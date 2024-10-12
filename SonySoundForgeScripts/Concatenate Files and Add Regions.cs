/* =======================================================================================================
 *	Script Name: Concatenate Files and Add Regions
 *	Description: Concatenate any number of media files with the same bit depth, sample rate, and 
 *	number of channels into a new file with a region added for each input file.
 *
 * ==================================================================================================== */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SoundForge;

public class EntryPoint
{
	public string Begin(IScriptableApp app)
	{

		//start MODIFY HERE-----------------------------------------------
		string szType = GETARG("type", ".flac"); //choose any valid extension: .avi  .wav  .w64 .mpg .mp3 .wma .mov .rm .aif .ogg .raw .au .dig .ivc .vox .pca
		object vPreset = GETARG("preset", ""); //put the name of the template between the quotes, or leave blank to pop the Template chooser.
		string szDir = GETARG("dir", ""); //hardcode a target path here

		// GETARG is a function that defines the default script settings. You can use the Script Args field to over-ride
		// the values within GETARG().
		// Example: To over-ride GETARG(Key, valueA), type Key=valueB in the Script Args field.
		//          Use an ampersand (&) to separate different Script Args: KeyOne=valueB&KeyTwo=valueC

		//Example Script Args: type=.wav&dir=f:\RegionFiles

		//end MODIFY HERE -----------------------------------

		string[] inputFiles = null;

		OpenFileDialog ofd = new OpenFileDialog();
		ofd.Filter = "All Files (*.*)|*.*";
		ofd.Multiselect = true;

		DialogResult result = ofd.ShowDialog(app.Win32Window);

		if (result == DialogResult.OK)
		{
			inputFiles = ofd.FileNames;
		}
		else
		{
			return "Did not select files to concatenate.";
		}

		app.WaitForDoneOrCancel();

		ISfFileHost outFile = null;
		List<string> skippedFiles = new List<string>();

		foreach (string filePath in inputFiles)
		{
			ISfFileHost inFile = app.OpenFile(filePath, true, false);
			
			if (outFile == null)
			{
				outFile = app.NewFile(inFile.DataFormat, false);
			}

			// result = MessageBox.Show(app.Win32Window, String.Join(",", "bit depth", inFile.DataFormat.BitDepth, "sample rate", inFile.DataFormat.SampleRate, "tag", inFile.DataFormat.WfxTag, "bytes per cell", inFile.DataFormat.BytesPerCell, "bytes per sample", inFile.DataFormat.BytesPerSample, "channels", inFile.DataFormat.Channels, "sample type", inFile.DataFormat.SampleType));

			if (inFile.Channels != outFile.Channels && inFile.DataFormat.BitDepth != outFile.DataFormat.BitDepth && inFile.DataFormat.SampleRate != outFile.DataFormat.SampleRate)
			{
				DPF("Skipping file in different format: '{0}'", inFile.Filename);
				skippedFiles.Add(inFile.Filename);
				inFile.Close(CloseOptions.DiscardChanges);
				continue;
			}

			outFile.OverwriteAudio(outFile.Length, 0, inFile, new SfAudioSelection(inFile));
			outFile.Markers.AddRegion(outFile.Length - inFile.Length, inFile.Length, Path.GetFileNameWithoutExtension(inFile.Filename));
			inFile.Close(CloseOptions.DiscardChanges);
		}

		if (skippedFiles.Count > 0)
		{
			result = MessageBox.Show(app.Win32Window, String.Join("\n", skippedFiles), "Skipped files in incorrect format");
		}

		return null;
	}

	public void FromSoundForge(IScriptableApp app)
	{
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
