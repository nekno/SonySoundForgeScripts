/* =======================================================================================================
 *	Script Name: Add Regions from CSV File
 *	Description: This script iterates through regions in a specified Comma Separated Values (plain text) 
 *	file and adds each region to the current file. Unlike opening a saved Regions (.sfl) file to apply 
 *	to an audio file, the process is additive and does not remove the current regions.
 *	
 *  Note: The CSV file should be of the format IDENT(Int64),NAME(string),START(Int64),LENGTH(Int64)
 *  
 *  Note: The files where the CSV regions are saved and applied must be of the same sample rate.
 *  The start position and length are represented in samples.
 *
 *	Initial State: Run with a file open where you want to apply regions.
 *
 * ==================================================================================================== */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SoundForge;

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
		if (null == file)
			return "Open a file before running this script. Script stopped.";

		string inputFile = null;
		OpenFileDialog ofd = new OpenFileDialog();
		ofd.Filter = "Comma Separated Values (*.csv)|*.csv";

		DialogResult result = ofd.ShowDialog(app.Win32Window);

		if (result == DialogResult.OK) {
			inputFile = ofd.FileName;
		} else {
			return "Did not select a file to add regions.";
		}

		string[] markers = File.ReadAllLines(inputFile);

		foreach (string marker in markers) {
			List<string> values = new List<string>(marker.Split(','));

			for (int i = 0; i < values.Count; i++) {
				string value = values[i];
				if (value.StartsWith("\"") && value.Length < i+1) {
					value = value.TrimStart('\"') + values[i+1].TrimEnd('\"');
					values[i] = value;
					values.RemoveAt(++i);
				}
			}

			string ident = values[0];
			string name = values[1];
			long start = Convert.ToInt64(values[2]);
			long length =  Convert.ToInt64(values[3]);

			file.Markers.AddRegion(start, length, name);

			DPF("Adding region: '{0}'", name);
		}

		return null;
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
