﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DesiredState.IIS;
using DesiredState.Windows;

namespace DesiredState.Common
{
	/// <summary>
	///  Coordinates generation of requested DSC
	/// </summary>
	internal class CodeGenerator
	{
		const string Indent = "     ";

		public string GenerateConfig(Options options)
		{
			IISCodeGenerator iis = new IISCodeGenerator();
			StringBuilder sb = new StringBuilder();

			string versionStr = GetApplicationVersion();

			string code;
			sb.AppendFormat("configuration IIS_DSC  # Generated by DSC Generator v{0}\n", versionStr);
			sb.AppendLine("{");

			if (options.IisOptions.IisPoolAndSitesGenerationMode != IISCodeGenerator.IisPoolAndSitesGenerationMode.NoGeneration)
			{
				code = iis.GenerateIisSiteImports();

				sb.AppendLine(code);
			}

			sb.AppendLine( Indent + "node localhost");
			sb.AppendLine( Indent + "{");

			if (options.GenerateIisWindowsFeatures)
			{
				var windowsGenerator = new WindowsFeatureCodeGenerator();

				code = windowsGenerator.GenerateWindowsFeatures();

				sb.Append(code);
			}

			if (options.IisOptions.IisPoolAndSitesGenerationMode != IISCodeGenerator.IisPoolAndSitesGenerationMode.NoGeneration)
			{
				code = iis.GenerateCode(options.IisOptions);
				sb.Append(code);
			}

			sb.AppendLine(  Indent + "}");
			sb.AppendLine(  "}\n");

			sb.AppendLine(  "cls\n");

			sb.AppendLine(  "# Compile this DSC down to an .MOF:  An .mof file will be placed in the specified directory:");
			sb.AppendLine(  "IIS_DSC -OutputPath \"c:\\my_iis_dsc_dir\"\n");

			sb.AppendLine(  "# Apply the DSC.  ALL .mof's in a folder will be executed!:");
			sb.AppendLine(  "Start-DscConfiguration -Path \"c:\\my_iis_dsc_dir\" -Wait -Debug -ErrorAction Stop -Force -Verbose  # this will apply the DSC");
			
			return sb.ToString();
		}

		private string GetApplicationVersion()
		{
			Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
			string version = fvi.FileVersion;
			return version;
		}

		internal struct Options
		{
			public bool GenerateIisWindowsFeatures;

			public IISCodeGenerator.IisPoolAndSitesOptions IisOptions;
		}

	}
}
