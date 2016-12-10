/* =======================================================================================================
 *	Script Name: Adjust Regions
 *	Description: This script iterates through regions in the open file and adjusts the start position
 *	+/- a specified number of samples. The length of each region is held constant, so the end position
 *	is also adjusted the same amount.
 *	
 *	Initial State: Run with a file open that contains regions.
 *
 * ==================================================================================================== */

using System;
using System.IO;
using System.Windows.Forms;
using SoundForge;

public class EntryPoint
{
	public string Begin(IScriptableApp app) {

		//start MODIFY HERE-----------------------------------------------
		long amount = GETARG("amount", 0);

		// GETARG is a function that defines the default script settings. You can use the Script Args field to over-ride
		// the values within GETARG().
		// Example: To over-ride GETARG(Key, valueA), type Key=valueB in the Script Args field.
		//          Use an ampersand (&) to separate different Script Args: KeyOne=valueB&KeyTwo=valueC

		//Example Script Args: type=.wav&dir=f:\RegionFiles

		//end MODIFY HERE -----------------------------------

		ISfFileHost file = app.CurrentFile;
		if (null == file)
			return "Open a file containing regions before running this script. Script stopped.";
		if (null == file.Markers || file.Markers.Count <= 0)
			return "The file does not have any markers.";

		if (amount <= 0) {
			string firstMarkerStart = String.Format("-{0}", file.Markers[0].Start);
			amount = Convert.ToInt64(SfHelpers.WaitForInputString("Enter the number of samples to adjust the regions (+/-):", firstMarkerStart));
		}

		foreach (SfAudioMarker mk in file.Markers) {
			mk.Start += amount;
			//mk.Length += amount;

			STATUS(String.Format("Marker: '{0}' Start: {1} End: {2}", mk.Name, mk.Start, mk.Start + mk.Length));
		}

		return null;
	}

	public void FromSoundForge(IScriptableApp app) {
		ForgeApp = app; //execution begins here
		app.SetStatusText(String.Format("Script '{0}' is running.", Script.Name));
		string msg = Begin(app);
		//app.SetStatusText((msg != null) ? msg : String.Format("Script '{0}' is done.", Script.Name));
	}
	public static IScriptableApp ForgeApp = null;
	public static void STATUS(string sz) { ForgeApp.SetStatusText(sz); }
	public static void DPF(string sz) { ForgeApp.OutputText(sz); }
	public static void DPF(string fmt, object o) { ForgeApp.OutputText(String.Format(fmt, o)); }
	public static void DPF(string fmt, object o, object o2) { ForgeApp.OutputText(String.Format(fmt, o, o2)); }
	public static void DPF(string fmt, object o, object o2, object o3) { ForgeApp.OutputText(String.Format(fmt, o, o2, o3)); }
	public static string GETARG(string k, string d) { string val = Script.Args.ValueOf(k); if (val == null || val.Length == 0) val = d; return val; }
	public static int GETARG(string k, int d) { string s = Script.Args.ValueOf(k); if (s == null || s.Length == 0) return d; else return Script.Args.AsInt(k); }
	public static bool GETARG(string k, bool d) { string s = Script.Args.ValueOf(k); if (s == null || s.Length == 0) return d; else return Script.Args.AsBool(k); }
} //EntryPoint
