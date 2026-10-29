#!/usr/bin/env bash
#
# Smoke test for the InkyCal.Server Docker image.
#
# Starts a SQL Server container plus the server image and waits until the
# /health endpoint reports healthy. On failure it prints a short, readable
# explanation followed by the container logs, and (when running in GitHub
# Actions) adds an error annotation and a job summary.
#
# Usage: scripts/smoke-test.sh <image> [timeout-seconds]
#
# Environment overrides:
#   SMOKE_DB_IMAGE   SQL Server image (default: mcr.microsoft.com/mssql/server:2022-latest)
#   SMOKE_HOST_PORT  Host port the server is published on (default: 18080)

set -uo pipefail

IMAGE="${1:-}"
TIMEOUT="${2:-180}"
DB_IMAGE="${SMOKE_DB_IMAGE:-mcr.microsoft.com/mssql/server:2022-latest}"
HOST_PORT="${SMOKE_HOST_PORT:-18080}"

if [[ -z "$IMAGE" ]]; then
	echo "Usage: $0 <image> [timeout-seconds]" >&2
	exit 2
fi

SUFFIX="$$"
NETWORK="inkycal-smoke-$SUFFIX"
DB="inkycal-smoke-db-$SUFFIX"
APP="inkycal-smoke-app-$SUFFIX"
DB_PASSWORD="Smoke-Test-$SUFFIX!"
HEALTH_URL="http://localhost:$HOST_PORT/health"

cleanup() {
	docker rm -f "$APP" "$DB" >/dev/null 2>&1
	docker network rm "$NETWORK" >/dev/null 2>&1
}
trap cleanup EXIT

log() { echo "[smoke-test] $*"; }

# Prints a clear failure report and exits.
#   $1: short title, $2: explanation, $3: container whose logs to show (optional)
fail() {
	local title="$1" reason="$2" container="${3:-}"
	local logs="" hint=""

	if [[ -n "$container" ]]; then
		logs="$(docker logs --tail 200 "$container" 2>&1)"
		# The first exception line is usually the most helpful hint
		hint="$(grep -m1 -E 'Exception|error:|Error:' <<<"$logs" | sed -E 's/^[[:space:]]+//')"
	fi

	echo
	echo "======================================================================"
	echo " SMOKE TEST FAILED: $title"
	echo "======================================================================"
	echo " Image:   $IMAGE"
	echo " Reason:  $reason"
	[[ -n "$hint" ]] && echo " Hint:    $hint"
	echo "======================================================================"

	if [[ -n "$container" ]]; then
		[[ -n "${GITHUB_ACTIONS:-}" ]] && echo "::group::Logs of $container (last 200 lines)"
		echo "--- Logs of $container (last 200 lines) ---"
		echo "$logs"
		echo "--- End of logs ---"
		[[ -n "${GITHUB_ACTIONS:-}" ]] && echo "::endgroup::"
	fi

	if [[ -n "${GITHUB_ACTIONS:-}" ]]; then
		# Annotations must be single-line; newlines are encoded as %0A
		local message="${reason//%/%25}"
		[[ -n "$hint" ]] && message+="%0AHint: ${hint//%/%25}"
		echo "::error title=Smoke test failed: $title::$message"

		if [[ -n "${GITHUB_STEP_SUMMARY:-}" ]]; then
			{
				echo "## :x: Smoke test failed: $title"
				echo
				echo "**Reason:** $reason"
				[[ -n "$hint" ]] && echo && echo "**Hint:** \`$hint\`"
				if [[ -n "$container" ]]; then
					echo
					echo "<details><summary>Last 50 log lines of <code>$container</code></summary>"
					echo
					echo '```'
					tail -n 50 <<<"$logs"
					echo '```'
					echo "</details>"
				fi
			} >>"$GITHUB_STEP_SUMMARY"
		fi
	fi

	exit 1
}

log "Creating network $NETWORK"
docker network create "$NETWORK" >/dev/null \
	|| fail "could not create Docker network" "'docker network create' failed; is Docker running?"

log "Starting SQL Server ($DB_IMAGE)"
docker run -d --name "$DB" --network "$NETWORK" \
	-e ACCEPT_EULA=Y \
	-e MSSQL_PID=Express \
	-e "MSSQL_SA_PASSWORD=$DB_PASSWORD" \
	"$DB_IMAGE" >/dev/null \
	|| fail "could not start SQL Server" "'docker run $DB_IMAGE' failed (see output above)."

log "Waiting for SQL Server to accept connections"
deadline=$((SECONDS + 120))
until docker logs "$DB" 2>&1 | grep -q "SQL Server is now ready for client connections"; do
	if [[ "$(docker inspect -f '{{.State.Running}}' "$DB" 2>/dev/null)" != "true" ]]; then
		fail "SQL Server stopped" "The database container exited before it was ready. This is a problem with the test setup, not with the server image." "$DB"
	fi
	if ((SECONDS >= deadline)); then
		fail "SQL Server did not start" "The database container was not ready within 120 seconds. This is a problem with the test setup, not with the server image." "$DB"
	fi
	sleep 2
done

log "Starting server image $IMAGE"
docker run -d --name "$APP" --network "$NETWORK" \
	-p "$HOST_PORT:8080" \
	-e ASPNETCORE_URLS=http://0.0.0.0:8080/ \
	-e "ConnectionStrings__DefaultConnection=Server=$DB;Database=InkyCal.Server;User=sa;Password=$DB_PASSWORD;MultipleActiveResultSets=true;Encrypt=False;" \
	"$IMAGE" >/dev/null \
	|| fail "could not start the container" "'docker run $IMAGE' failed; does the image exist locally?"

log "Waiting up to ${TIMEOUT}s for $HEALTH_URL to report healthy"
deadline=$((SECONDS + TIMEOUT))
status=""
body=""
while true; do
	if [[ "$(docker inspect -f '{{.State.Running}}' "$APP" 2>/dev/null)" != "true" ]]; then
		exit_code="$(docker inspect -f '{{.State.ExitCode}}' "$APP" 2>/dev/null)"
		fail "container exited during startup" "The server container stopped with exit code $exit_code before becoming healthy. The application most likely crashed during startup; see the hint and logs below." "$APP"
	fi

	body="$(curl -s --max-time 5 -w $'\n%{http_code}' "$HEALTH_URL" 2>/dev/null)"
	status="${body##*$'\n'}"
	body="${body%$'\n'*}"

	if [[ "$status" == "200" ]]; then
		log "Healthy: $body"
		log "PASSED"
		[[ -n "${GITHUB_STEP_SUMMARY:-}" ]] \
			&& echo "## :white_check_mark: Smoke test passed: \`$IMAGE\` started and \`/health\` returned 200" >>"$GITHUB_STEP_SUMMARY"
		exit 0
	fi

	if ((SECONDS >= deadline)); then
		case "$status" in
			000 | "") reason="The server did not respond on $HEALTH_URL within ${TIMEOUT}s; it may still be starting, hanging, or listening on a different port." ;;
			417) reason="/health kept reporting Degraded (HTTP 417) for ${TIMEOUT}s, meaning the server runs but cannot reach its database. Response: $body" ;;
			503) reason="/health kept reporting Unhealthy (HTTP 503) for ${TIMEOUT}s. Response: $body" ;;
			*) reason="/health returned HTTP $status instead of 200 after ${TIMEOUT}s. Response: $body" ;;
		esac
		fail "server did not become healthy" "$reason" "$APP"
	fi

	sleep 3
done
