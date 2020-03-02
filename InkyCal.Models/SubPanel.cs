﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	[Table("SubPanel", Schema = "InkyCal")]
	public class SubPanel
	{

		[Key, Required]
		public Guid IdParent { get; set; }

		[Key, Required]
		public Guid IdPanel { get; set; }

		[ForeignKey(nameof(IdParent))]
		public virtual Panel Panel { get; set; }

		[Key]
		public byte SortIndex { get; set; }
	}
}
