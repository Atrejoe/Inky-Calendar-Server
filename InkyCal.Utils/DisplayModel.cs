using SixLabors.ImageSharp;
using System;

namespace InkyCal.Utils
{
    /// <summary>
    /// All supported E-Ink panels
    /// </summary>
    public enum DisplayModel
    {
        /// <summary>
        /// 7.5" high-res black-white-red/yellow
        /// </summary>
        epd_7_in_5_v2_colour,
        /// <summary>
        /// 7.5" high-res black-white
        /// </summary>
        epd_7_in_5_v2,
        /// <summary>
        /// 7.5" black-white-red/yellow
        /// </summary>
        epd_7_in_5_colour,
        /// <summary>
        /// 7.5" black-white
        /// </summary>
        epd_7_in_5,
        /// <summary>
        /// 5.83" black-white-red/yellow
        /// </summary>
        epd_5_in_83_colour,
        /// <summary>
        /// 5.83" black-white
        /// </summary>
        epd_5_in_83,
        /// <summary>
        /// 4.2" black-white-red/yellow
        /// </summary>
        epd_4_in_2_colour,
        /// <summary>
        /// 4.2" black-white
        /// </summary>
        epd_4_in_2,// # 
    }

    /// <summary>
    /// A helper class for obtaining display model properties
    /// </summary>
    public static class DisplayModelHelper
    {

        /// <summary>
        /// Gets the specs for the <paramref name="model"/>.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colors"></param>
        public static void GetSpecs(this DisplayModel model, out int width, out int height, out Color[] colors)
        {

            switch (model)
            {
                case DisplayModel.epd_7_in_5_v2_colour:
                case DisplayModel.epd_7_in_5_v2:
                    width = 800;
                    height = 480;
                    break;
                case DisplayModel.epd_7_in_5_colour:
                case DisplayModel.epd_7_in_5:
                    width = 640;
                    height = 384;
                    break;
                case DisplayModel.epd_5_in_83_colour:
                case DisplayModel.epd_5_in_83:
                    width = 600;
                    height = 448;
                    break;
                case DisplayModel.epd_4_in_2_colour:
                case DisplayModel.epd_4_in_2:
                    width = 400;
                    height = 300;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(model), model, $"Model `{model}` is not supported, dimensions unknown.");
            }

            switch (model)
            {
                case DisplayModel.epd_7_in_5_v2_colour:
                case DisplayModel.epd_7_in_5_colour:
                    colors = new[] { Color.Black, Color.White, Color.Red }; //Could be yellow too, maybe introdue a new panel type
                    break;
                case DisplayModel.epd_7_in_5_v2:
                case DisplayModel.epd_7_in_5:
                    colors = new[] { Color.Black, Color.White };
                    break;
                case DisplayModel.epd_5_in_83_colour:
                case DisplayModel.epd_4_in_2_colour:
                    colors = new[] { Color.Black, Color.White, Color.Red };
                    break;
                case DisplayModel.epd_5_in_83:
                case DisplayModel.epd_4_in_2:
                    colors = new[] { Color.Black, Color.White };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(model), model, $"Model `{model}` is not supported, colors unknown.");
            }
        }
    }
}
