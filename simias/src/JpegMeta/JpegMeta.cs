/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
/*
 * Implements dredging of JPEG information from a specified file
 * and returning it to be stored as metadata on a Node in the
 * Collection Store.
 */

using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Novell.iFolder.Storage
{
	/// <summary>
	/// Class that parses the EXIF header and builds a readable name-value list.
	/// </summary>
	class JpegMetaData
	{
		#region Class Members
		/// <summary>
		/// Exif data types.
		/// </summary>
		private enum ExifType
		{
			/// <summary>
			/// An 8-bit unsigned integer.
			/// </summary>
			Byte = 1,

			/// <summary>
			/// An 8-bit byte containing one 7-bit ASCII code. The final byte is terminated with NULL.
			/// </summary>
			ASCII = 2,

			/// <summary>
			/// A 16-bit (2 -byte) unsigned integer.
			/// </summary>
			Short = 3,

			/// <summary>
			/// A 32-bit (4 -byte) unsigned integer.
			/// </summary>
			Long = 4,

			/// <summary>
			/// Two LONGs. The first LONG is the numerator and the second LONG expresses the denominator.
			/// </summary>
			Rational = 5,

			/// <summary>
			/// An 8-bit byte that can take any value depending on the field definition.
			/// </summary>
			Undefined = 7,

			/// <summary>
			/// A 32-bit (4 -byte) signed integer (2's complement notation).
			/// </summary>
			SLong = 9,

			/// <summary>
			/// Two SLONGs. The first SLONG is the numerator and the second SLONG is the denominator.
			/// </summary>
			SRational = 10	
		};

		/// <summary>
		/// EXIF identifiers for interesting data.
		/// </summary>
		private enum ExifId
		{
			EquipmentMake = 271,
			CameraModel = 272,
			CreationSoftware = 305,
			ExposureTime = 33434,
			FNumber = 33437,
			ExposureProgram = 34850,
			ISOSpeed = 34855,
			DatePictureTaken = 36867,
			SubjectDistance = 37382,
			MeteringMode = 37383,
			LightSource = 37384,
			FlashMode = 37385,
			FocalLength = 37386,
			ShutterSpeedValue = 37377,
			ApertureValue = 37378,
			BrightnessValue = 37379,
			ExposureCompensation = 37380,
			Title = 40091,
			Comments = 40092,
			Author = 40093,
			Keywords = 40094,
			Subject = 40095,
			ColorSpace = 40961,
			WhiteBalance = 41987,
			ExposureMode = 41986,
			Contrast = 41992,
			Saturation = 41993,
			Sharpness = 41994
		};

		/// <summary>
		/// Object that represents the JPEG image.
		/// </summary>
		private Image image;

		/// <summary>
		/// Array that holds the property item Id's for this image.
		/// </summary>
		private int[] itemIdArray;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for object.
		/// </summary>
		/// <param name="fileName">Path to JPEG file.</param>
		public JpegMetaData( string fileName )
		{
			image = Image.FromFile( fileName );
			itemIdArray = image.PropertyIdList;
			Array.Sort( itemIdArray );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the aperture display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetApertureAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ApertureValue );
			if ( item != null )
			{
				return GetExifRationalValue( item.Value ).ToString( "F0" );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the author display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetAuthorAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Author );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, true );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the bit depth display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetBitDepthAsString()
		{
			return Image.GetPixelFormatSize( image.PixelFormat ).ToString();
		}

		/// <summary>
		/// Gets the brightness display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetBrightnessAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.BrightnessValue );
			if ( item != null )
			{
				return GetExifSRationalValue( item.Value ).ToString( "F0" );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the camera model display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetCameraModelAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.CameraModel );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, false );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the color specification display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetColorSpaceAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ColorSpace );
			if ( item != null )
			{
				if ( GetExifShortValue( item.Value ) == 1 )
				{
					return "sRGB";
				}
				else
				{
					return "Uncalibrated";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the comments display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetCommentsAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Comments );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, true );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the contrast display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetContrastAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Contrast );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Normal";

					case 1:
						return "Soft";

					case 2:
						return "Hard";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the creation software display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetCreationSoftwareAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.CreationSoftware );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, false );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the date the picture was taken display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetDatePictureTakenAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.DatePictureTaken );
			if ( item != null )
			{
				string date = GetExifStringValue( item.Value, false );
				DateTime dateTime = new DateTime( Convert.ToInt32( date.Substring( 0, 4 ) ),
					Convert.ToInt32( date.Substring( 5, 2 ) ),
					Convert.ToInt32( date.Substring( 8, 2 ) ),
					Convert.ToInt32( date.Substring( 11, 2 ) ),
					Convert.ToInt32( date.Substring( 14, 2 ) ),
					Convert.ToInt32( date.Substring( 17, 2 ) ) );
				return dateTime.ToString( "g" );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the equipment make display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetEquipmentMakeAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.EquipmentMake );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, false );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the unsigned byte value from a formatted EXIF byte array.
		/// </summary>
		/// <param name="arrayValue">EXIF formatted byte array.</param>
		/// <returns>An unsigned byte that represents the array value.</returns>
		private byte GetExifByteValue( byte[] arrayValue )
		{
			return arrayValue[ 0 ];
		}

		/// <summary>
		/// Gets the unsigned int value from a formatted EXIF byte array.
		/// </summary>
		/// <param name="arrayValue">EXIF formatted byte array.</param>
		/// <param name="offset">Offset into array where value starts.</param>
		/// <returns>An unsigned int that represents the array value.</returns>
		private uint GetExifUIntValue( byte[] arrayValue, int offset )
		{
			return BitConverter.ToUInt32( arrayValue, offset );
		}

		/// <summary>
		/// Gets the unsigned rational value from a formatted EXIF byte array.
		/// </summary>
		/// <param name="arrayValue">EXIF formatted byte array.</param>
		/// <returns>A float that represents the array value.</returns>
		private float GetExifRationalValue( byte[] arrayValue )
		{
			uint numerator = BitConverter.ToUInt32( arrayValue, 0 );
			uint denominator = BitConverter.ToUInt32( arrayValue, 4 );
			return Convert.ToSingle( numerator ) / Convert.ToSingle( denominator );
		}

		/// <summary>
		/// Gets the unsigned short value from a formatted EXIF byte array.
		/// </summary>
		/// <param name="arrayValue">EXIF formatted byte array.</param>
		/// <returns>An unsigned short that represents the array value.</returns>
		private ushort GetExifShortValue( byte[] arrayValue )
		{
			return BitConverter.ToUInt16( arrayValue, 0 );
		}

		/// <summary>
		/// Gets the signed rational value from a formatted EXIF byte array.
		/// </summary>
		/// <param name="arrayValue">EXIF formatted byte array.</param>
		/// <returns>A float that represents the array value.</returns>
		private float GetExifSRationalValue( byte[] arrayValue )
		{
			int numerator = BitConverter.ToInt32( arrayValue, 0 );
			int denominator = BitConverter.ToInt32( arrayValue, 4 );
			return Convert.ToSingle( numerator ) / Convert.ToSingle( denominator );
		}

		/// <summary>
		/// Gets the string value from a formatted EXIF byte array.
		/// </summary>
		/// <param name="arrayValue">EXIF formatted byte array.</param>
		/// <param name="isWideCharFormat">If true byte array is in wide character format.  Otherwise it is in single byte format.</param>
		/// <returns>A string that represents the array value.</returns>
		private string GetExifStringValue( byte[] arrayValue, bool isWideCharFormat )
		{
			if ( isWideCharFormat )
			{
				return new UnicodeEncoding().GetString( arrayValue );
			}
			else
			{
				return new ASCIIEncoding().GetString( arrayValue );
			}
		}

		/// <summary>
		/// Gets the exposure compensation display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetExposureCompensationAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ExposureCompensation );
			if ( item != null )
			{
				float exposureComp = GetExifSRationalValue( item.Value );
				return ( ( exposureComp == 0 ) ? "0" : exposureComp.ToString( "F1" ) ) + " step";
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the exposure mode display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetExposureModeAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ExposureMode );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Auto";

					case 1:
						return "Manual";

					case 2:
						return "Bracket";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the exposure program display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetExposureProgramAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ExposureProgram );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Not defined";

					case 1:
						return "Manual";

					case 2:
						return "Normal";

					case 3:
						return "Aperture Priority";

					case 4:
						return "Shutter Priority";

					case 5:
						return "Creative";

					case 6:
						return "Action";

					case 7:
						return "Portrait";

					case 8:
						return "Landscape";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the exposure time display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetExposureTimeAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ExposureTime );
			if ( item != null )
			{
				return GetExifUIntValue( item.Value, 0 ).ToString() + "/" + GetExifUIntValue( item.Value, 4 ).ToString() + " sec.";
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the flash mode display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetFlashModeAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.FlashMode );
			if ( item != null )
			{
				if ( ( GetExifByteValue( item.Value ) & 0x01 ) == 0x01 )
				{
					return "Flash fired";
				}
				else
				{
					return "Flash did not fire";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the fstop number display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetFNumberAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.FNumber );
			if ( item != null )
			{
				return "F/" + GetExifRationalValue( item.Value ).ToString( "F1" );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the focal length display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetFocalLengthAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.FocalLength );
			if ( item != null )
			{
				return GetExifRationalValue( item.Value ).ToString( "F0" ) + " mm";
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the frame count display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetFrameCountAsString()
		{
			return image.GetFrameCount( new FrameDimension( image.FrameDimensionsList[ 0 ] ) ).ToString();
		}

		/// <summary>
		/// Gets the height display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetHeightAsString()
		{
			return image.Height.ToString() + " pixels";
		}

		/// <summary>
		/// Gets the horizontal resolution display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetHorizontialResolutionAsString()
		{
			return image.HorizontalResolution.ToString() + " dpi";
		}

		/// <summary>
		/// Gets the ISO speed display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetISOSpeedAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ISOSpeed );
			if ( item != null )
			{
				return "ISO-" + GetExifShortValue( item.Value ).ToString();
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the keywords display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetKeywordsAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Keywords );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, true );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the light source display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetLightSourceAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.LightSource );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Unknown";

					case 1:
						return "Daylight";

					case 2:
						return "Fluorescent";

					case 3:
						return "Tungsten";

					case 17:
						return "Standard light A";

					case 18:
						return "Standard light B";

					case 19:
						return "Standard light C";

					case 20:
						return "D55";

					case 21:
						return "D65";

					case 22:
						return "D75";

					case 255:
						return "Other";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the metering mode display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetMeteringModeAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.MeteringMode );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Unknown";

					case 1:
						return "Average";

					case 2:
						return "CenterWeightedAverage";

					case 3:
						return "Spot";

					case 4:
						return "MultiSpot";

					case 5:
						return "Pattern";

					case 6:
						return "Partial";

					case 255:
						return "Other";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the specified property item if it exists.
		/// </summary>
		/// <param name="propertyId">EXIF property identifier.</param>
		/// <returns>A property item object that represents the specified property.</returns>
		private PropertyItem GetPropertyItem( ExifId propertyId )
		{
			// See if the property exists on the image.
			int index = Array.BinarySearch( itemIdArray, ( int )propertyId );
			if ( index >= 0 )
			{
				return image.GetPropertyItem( ( int )propertyId );
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the saturation display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetSaturationAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Saturation );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Normal";

					case 1:
						return "Low";

					case 2:
						return "High";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the sharpness display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetSharpnessAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Sharpness );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Normal";

					case 1:
						return "Soft";

					case 2:
						return "Hard";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the shutter speed display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetShutterSpeedAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.ShutterSpeedValue );
			if ( item != null )
			{
				return GetExifSRationalValue( item.Value ).ToString( "F0" );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the subject display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetSubjectAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Subject );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, true );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the subject distance display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetSubjectDistanceAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.SubjectDistance );
			if ( item != null )
			{
				return GetExifRationalValue( item.Value ).ToString( "F1" ) + " m";
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the title display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetTitleAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.Title );
			if ( item != null )
			{
				return GetExifStringValue( item.Value, true );
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the vertical resolution display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetVerticalResolutionAsString()
		{
			return image.VerticalResolution.ToString() + " dpi";
		}

		/// <summary>
		/// Gets the white balance display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetWhiteBalanceAsString()
		{
			PropertyItem item = GetPropertyItem( ExifId.WhiteBalance );
			if ( item != null )
			{
				switch ( GetExifShortValue( item.Value ) )
				{
					case 0:
						return "Auto";

					case 1:
						return "Manual";

					default:
						return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the width display string.
		/// </summary>
		/// <returns>A string containing the display string.</returns>
		private string GetWidthAsString()
		{
			return image.Width.ToString() + " pixels";
		}
		#endregion

		#region Public Members
		/// <summary>
		/// Gets the metadata information for a JPEG file.
		/// </summary>
		/// <param name="imageFilePath">Path to the JPEG file.</param>
		/// <returns>A StringDictionary object containing the metadata values.</returns>
		static public StringDictionary GetMetaData( string imageFilePath )
		{
			try
			{
				JpegMetaData jpeg = new JpegMetaData( imageFilePath );
				StringDictionary nameValues = new StringDictionary();

				nameValues.Add( "Width",                 jpeg.GetWidthAsString() );
				nameValues.Add( "Height",                jpeg.GetHeightAsString() );
				nameValues.Add( "Horizontal Resolution", jpeg.GetHorizontialResolutionAsString() );
				nameValues.Add( "Vertical Resolution",   jpeg.GetVerticalResolutionAsString() );
				nameValues.Add( "Bit Depth",             jpeg.GetBitDepthAsString() );
				nameValues.Add( "Frame Count",           jpeg.GetFrameCountAsString() );
				nameValues.Add( "Equipment Make",        jpeg.GetEquipmentMakeAsString() );
				nameValues.Add( "Camera Model",          jpeg.GetCameraModelAsString() );
				nameValues.Add( "Creation Software",     jpeg.GetCreationSoftwareAsString() );
				nameValues.Add( "Color Representation",  jpeg.GetColorSpaceAsString() );
				nameValues.Add( "Flash Mode",            jpeg.GetFlashModeAsString() );
				nameValues.Add( "Focal Length",          jpeg.GetFocalLengthAsString() );
				nameValues.Add( "F-Number",              jpeg.GetFNumberAsString() );
				nameValues.Add( "Exposure Time",         jpeg.GetExposureTimeAsString() );
				nameValues.Add( "ISO Speed",             jpeg.GetISOSpeedAsString() );
				nameValues.Add( "Metering Mode",         jpeg.GetMeteringModeAsString() );
				nameValues.Add( "Light Source",          jpeg.GetLightSourceAsString() );
				nameValues.Add( "Exposure Program",      jpeg.GetExposureProgramAsString() );
				nameValues.Add( "Exposure Compensation", jpeg.GetExposureCompensationAsString() );
				nameValues.Add( "Date Picture Taken",    jpeg.GetDatePictureTakenAsString() );
				nameValues.Add( "Title",                 jpeg.GetTitleAsString() );
				nameValues.Add( "Subject",               jpeg.GetSubjectAsString() );
				nameValues.Add( "Keywords",              jpeg.GetKeywordsAsString() );
				nameValues.Add( "Comments",              jpeg.GetCommentsAsString() );
				nameValues.Add( "Author",                jpeg.GetAuthorAsString() );
				nameValues.Add( "Subject Distance",      jpeg.GetSubjectDistanceAsString() );
				nameValues.Add( "Shutter Speed",         jpeg.GetShutterSpeedAsString() );
				nameValues.Add( "Aperture",              jpeg.GetApertureAsString() );
				nameValues.Add( "Brightness",            jpeg.GetBrightnessAsString() );
				nameValues.Add( "Exposure Mode",         jpeg.GetExposureModeAsString() );
				nameValues.Add( "White Balance",         jpeg.GetWhiteBalanceAsString() );
				nameValues.Add( "Contrast",              jpeg.GetContrastAsString() );
				nameValues.Add( "Saturation",            jpeg.GetSaturationAsString() );
				nameValues.Add( "Sharpness",             jpeg.GetSharpnessAsString() );

				return nameValues;
			}
			catch
			{
				return null;
			}
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			StringDictionary sd = JpegMetaData.GetMetaData( args[ 0 ] );
			foreach ( string key in sd.Keys )
			{
				Console.WriteLine( key + " = " + sd[ key ] );
			}
		}
	}
}
