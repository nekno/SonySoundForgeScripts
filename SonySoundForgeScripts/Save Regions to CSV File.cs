/* =======================================================================================================
 *	Script Name: Save Regions to CSV File
 *	Description: This script iterates through regions in the open file and saves each region 
 *	to a specified file in Comma Separated Values format (plain text).
 *	
 *  Note: The CSV file will be of the format IDENT(Int64),NAME(string),START(Int64),LENGTH(Int64)
 *  
 *  Note: The files where the CSV regions are saved and applied must be of the sample rate.
 *  The start position and length are represented in samples.
 *
 *	Initial State: Run with a file open that contains regions.
 *
 * ==================================================================================================== */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SoundForge;

//Run with a file that contains regions
//Iterates through the regions, renders to PCA and saves the rendered file to c:\media\rip
//Scan the file for MODIFY HERE to see how to quickly customize for your own use

public class EntryPoint
{
	public string Begin(IScriptableApp app) {

		//start MODIFY HERE-----------------------------------------------
		string szType  = GETARG("type", ".flac"); //choose any valid extension: .avi  .wav  .w64 .mpg .mp3 .wma .mov .rm .aif .ogg .raw .au .dig .ivc .vox .pca
		object vPreset = GETARG("preset", ""); //put the name of the template between the quotes, or leave blank to pop the Template chooser.
		string szDir   = GETARG("dir", ""); //hardcode a target path here

		// GETARG is a function that defines the default script settings. You can use the Script Args field to over-ride
		// the values within GETARG().
		// Example: To over-ride GETARG(Key, valueA), type Key=valueB in the Script Args field.
		//          Use an ampersand (&) to separate different Script Args: KeyOne=valueB&KeyTwo=valueC

		//Example Script Args: type=.wav&dir=f:\RegionFiles

		//end MODIFY HERE -----------------------------------



		ISfFileHost file = app.CurrentFile;
		string outputFile = null;
		if (null == file)
			return "Open a file containing regions before running this script. Script stopped.";
		if (null == file.Markers || file.Markers.Count <= 0)
			return "The file does not have any markers.";

		SaveFileDialog sfd = new SaveFileDialog();
		sfd.Filter = "Comma Separated Values (*.csv)|*.csv";

		DialogResult result = sfd.ShowDialog(app.Win32Window);

		if (result == DialogResult.OK) {
			outputFile = sfd.FileName;
		} else {
			return "Did not select a file to save regions.";
		}

		List<String> markers = new List<string>();

		foreach (SfAudioMarker mk in file.Markers)
		{
			if (!mk.IsRegion)
				continue;

			markers.Add(String.Join(",", new string[] { mk.Ident.ToString(), quoted(mk.Name), mk.Start.ToString(), mk.Length.ToString() }));

			DPF("Adding: '{0}'", mk.Name);
		}

		File.WriteAllLines(outputFile, markers.ToArray());

   return null;
}

	private string quoted(string input) {
		if (input.Contains(",")) {
			return String.Format("\"{0}\"", input);
		} else {
			return input;
		}
	}

	public void FromSoundForge(IScriptableApp app) {
		ForgeApp = app; //execution begins here
		app.SetStatusText(String.Format("Script '{0}' is running.", Script.Name));
		string msg = Begin(app);
		app.SetStatusText((msg != null) ? msg : String.Format("Script '{0}' is done.", Script.Name));
	}
	public static IScriptableApp ForgeApp = null;
	public static void DPF(string sz) { ForgeApp.OutputText(sz); }
	public static void DPF(string fmt, object o) { ForgeApp.OutputText(String.Format(fmt, o)); }
	public static void DPF(string fmt, object o, object o2) { ForgeApp.OutputText(String.Format(fmt, o, o2)); }
	public static void DPF(string fmt, object o, object o2, object o3) { ForgeApp.OutputText(String.Format(fmt, o, o2, o3)); }
	public static string GETARG(string k, string d) { string val = Script.Args.ValueOf(k); if (val == null || val.Length == 0) val = d; return val; }
	public static int GETARG(string k, int d) { string s = Script.Args.ValueOf(k); if (s == null || s.Length == 0) return d; else return Script.Args.AsInt(k); }
	public static bool GETARG(string k, bool d) { string s = Script.Args.ValueOf(k); if (s == null || s.Length == 0) return d; else return Script.Args.AsBool(k); }
} //EntryPoint
