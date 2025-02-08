using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.VectorTypes;
using NAudio.Wave;
using System.Drawing.Drawing2D;

namespace UltimateManagedCuda.Audio
{
	public class AudioHandling
	{
		// ~~~~~ ~~~~~ ~~~~~ ATTRIBUTES ~~~~~ ~~~~~ ~~~~~ \\
		public ListBox? TrackBox;
		public Label? TracksInfoLabel;
		public Label? PointersInfoLabel;


		public List<AudioObject> Tracks = [];




		// ~~~~~ ~~~~~ ~~~~~ CONSTRUCTORS ~~~~~ ~~~~~ ~~~~~ \\
		public AudioHandling(ListBox? trackBox = null, Label? tracksInfo = null, Label? pointersInfo = null)
		{
			TrackBox = trackBox;
			TracksInfoLabel = tracksInfo;
			PointersInfoLabel = pointersInfo;
		}




		// ~~~~~ ~~~~~ ~~~~~ METHODS ~~~~~ ~~~~~ ~~~~~ \\
		public List<AudioObject> UpdateTracksList()
		{
			// Abort if box is not set
			if (TrackBox == null)
			{
				return Tracks;
			}

			// Clear box
			TrackBox.Items.Clear();
			List<int> remove = [];

			// Add tracks to box
			foreach (AudioObject track in Tracks)
			{
				string name = track.Name;

				// Abort if pointer & data are null
				if (track.Pointer == 0 && track.Spectrum == 0 && track.Data.Length == 0)
				{
					remove.Add(Tracks.IndexOf(track));
					continue;
				}

				if (track.Pointer != 0)
				{
					name = " ☆ " + name;
				}

				if (track.Spectrum != 0)
				{
					name = " ★ " + name;
				}

				if (name.Length > 50)
				{
					name = name.Substring(0, 50) + "...";
				}

				TrackBox.Items.Add(name);
			}

			// Remove tracks with invalid data
			foreach (int index in remove)
			{
				Tracks.RemoveAt(index);
			}

			// Update labels if set
			if (TracksInfoLabel != null)
			{
				TracksInfoLabel.Text = "Tracks (" + Tracks.Count + ")";
			}

			// Return tracks
			return Tracks;
		}

		public void AddTrack(string filepath)
		{
			Tracks.Add(new AudioObject(filepath));

			// Update listbox
			UpdateTracksList();
		}

		public int MakeTrack(float[] data, string name, int samplerate = 44100, int bitdepth = 32, int channels = 2)
		{
			// Create new track
			AudioObject track = new("")
			{
				// Set attributes
				Data = data,
				Name = name,
				Samplerate = samplerate,
				Bitdepth = bitdepth,
				Channels = channels,
				Length = data.Length,
				Duration = data.Length / samplerate
			};

			// Normalize data by dividing by count of samples
			for (int i = 0; i < track.Data.Length; i++)
			{
				track.Data[i] /= track.Data.Length;
			}

			// Add track
			Tracks.Add(track);

			// Get index
			int index = Tracks.IndexOf(track);

			// Update listbox
			UpdateTracksList();

			// Return index
			return index;
		}

		public void RemoveTrack(int index)
		{
			if (index >= 0 && index < Tracks.Count)
			{
				Tracks[index].Data = [];
				Tracks.RemoveAt(index);

				// GC
				GC.Collect();
			}
		}

		public void RemoveTrack(long pointer)
		{
			foreach (AudioObject track in Tracks)
			{
				if (track.Pointer == pointer)
				{
					track.Pointer = 0;
				}

				if (track.Spectrum == pointer)
				{
					track.Spectrum = 0;
				}
			}

			// GC
			GC.Collect();

			// Update listbox
			UpdateTracksList();
		}

		public string ExportTrack(int index, string dir)
		{
			string filepath = "Invalid";

			// Abort if index is invalid
			if (index < 0 || index >= Tracks.Count || !Directory.Exists(dir))
			{
				return filepath;
			}

			// Get track
			AudioObject track = Tracks[index];

			// Add waveheader
			byte[] data = track.AddWaveheader();

			// Export track
			if (data.Length > 0)
			{
				string name = track.Name.Replace("<", "").Replace(">", "");
				filepath = dir + "\\" + name + ".wav";
				File.WriteAllBytes(filepath, data);
			}

			// Return filepath
			return filepath;
		}

		public float2[] StretchData(float2[] fftForm, float factor)
		{
			// Abort if length is 0 or factor is 1
			if (fftForm.Length == 0 || factor == 1.0f || factor < 0.1f)
			{
				return fftForm;
			}

			// Get new size & array
			int newSize = (int) (fftForm.Length * factor);
			float2[] stretched = new float2[newSize];

			// Stretch data
			for (int i = 0; i < newSize; i++)
			{
				float index = i / factor;
				int lower = (int) Math.Floor(index);
				int upper = Math.Min(lower + 1, fftForm.Length - 1); // Verhindert Out-of-Bounds

				float fraction = index - lower;

				// Lineare Interpolation für Real- und Imaginärteil
				stretched[i].x = fftForm[lower].x + fraction * (fftForm[upper].x - fftForm[lower].x);
				stretched[i].y = fftForm[lower].y + fraction * (fftForm[upper].y - fftForm[lower].y);
			}

			// Return stretched data
			return stretched;
		}

		public float[] StretchMagnitudes(float[] magnitudeForm, float factor)
		{
			if (magnitudeForm.Length == 0 || factor == 1.0f || factor < 0.1f)
			{
				return magnitudeForm;
			}

			int numSamples = magnitudeForm.Length;
			int numChannels = 2; // Stereo: Left + Right
			int originalSize = numSamples / numChannels;
			int newSize = (int) (originalSize * factor);
			float[] stretched = new float[newSize * numChannels];

			for (int ch = 0; ch < numChannels; ch++)
			{
				for (int i = 0; i < newSize; i++)
				{
					float index = i / factor;
					int lower = (int) Math.Floor(index);
					int upper = Math.Min(lower + 1, originalSize - 1);
					float fraction = index - lower;

					int lowerIndex = lower * numChannels + ch;
					int upperIndex = upper * numChannels + ch;
					int newIndex = i * numChannels + ch;

					// Interpolation für diesen Kanal
					stretched[newIndex] = magnitudeForm[lowerIndex] + fraction * (magnitudeForm[upperIndex] - magnitudeForm[lowerIndex]);
				}
			}

			return stretched;
		}





		public class AudioObject
		{
			// ~~~~~ ~~~~~ ~~~~~ ATTRIBUTES ~~~~~ ~~~~~ ~~~~~ \\
			public WaveOutEvent Player;

			public string Filepath;
			public string Name;

			public int Samplerate = 44100;
			public int Bitdepth = 16;
			public int Channels = 2;

			public long Length = 0;
			public double Duration = 0.0;

			public float[] Data = [];
			public long Pointer = 0;
			public long Spectrum = 0;




			// ~~~~~ ~~~~~ ~~~~~ CONSTRUCTORS ~~~~~ ~~~~~ ~~~~~ \\
			public AudioObject(string filepath)
			{
				// New player
				Player = new WaveOutEvent();

				// Set filepath and name
				Filepath = filepath;
				Name = Path.GetFileNameWithoutExtension(Filepath);

				// Abort if file does not exist or isnt .wav, .mp3, .flac
				if (!File.Exists(Filepath) || !Filepath.EndsWith(".wav") && !Filepath.EndsWith(".mp3") && !Filepath.EndsWith(".flac"))
				{
					Name = "Invalid file";
					return;
				}

				// New AudioFileReader
				AudioFileReader reader = new(Filepath);

				// Set attributes
				Samplerate = reader.WaveFormat.SampleRate;
				Bitdepth = reader.WaveFormat.BitsPerSample;
				Channels = reader.WaveFormat.Channels;
				Length = reader.Length;
				Duration = reader.TotalTime.TotalSeconds;

				// Read data
				Data = new float[Length];
				int read = reader.Read(Data, 0, (int) Length);

				// Dispose reader
				reader.Dispose();
			}




			// ~~~~~ ~~~~~ ~~~~~ METHODS ~~~~~ ~~~~~ ~~~~~ \\
			public byte[] GetBytes()
			{
				int bytesPerSample = Bitdepth / 8;
				byte[] bytes = new byte[Data.Length * bytesPerSample];

				for (int i = 0; i < Data.Length; i++)
				{
					byte[] byteArray;
					float sample = Data[i];

					switch (Bitdepth)
					{
						case 16:
							short shortSample = (short) (sample * short.MaxValue);
							byteArray = BitConverter.GetBytes(shortSample);
							break;
						case 24:
							int intSample24 = (int) (sample * (1 << 23));
							byteArray = new byte[3];
							byteArray[0] = (byte) (intSample24 & 0xFF);
							byteArray[1] = (byte) ((intSample24 >> 8) & 0xFF);
							byteArray[2] = (byte) ((intSample24 >> 16) & 0xFF);
							break;
						case 32:
							int intSample32 = (int) (sample * int.MaxValue);
							byteArray = BitConverter.GetBytes(intSample32);
							break;
						default:
							throw new ArgumentException("Unsupported bit depth");
					}

					Buffer.BlockCopy(byteArray, 0, bytes, i * bytesPerSample, bytesPerSample);
				}

				return bytes;
			}

			public float[] GetFloats(byte[] bytes)
			{
				// Abort if no data
				if (bytes.Length == 0)
				{
					return [];
				}

				// Get bytes per sample & create float array
				int bytesPerSample = Bitdepth / 8;
				float[] floats = new float[bytes.Length / bytesPerSample / Channels];

				// Convert bytes to floats
				for (int i = 0; i < floats.Length / Channels; i++)
				{
					byte[] byteArray = new byte[bytesPerSample];
					Buffer.BlockCopy(bytes, i * bytesPerSample, byteArray, 0, bytesPerSample);
					switch (Bitdepth)
					{
						case 16:
							short shortSample = BitConverter.ToInt16(byteArray, 0);
							floats[i] = shortSample / (float) short.MaxValue;
							break;
						case 24:
							int intSample24 = byteArray[0] | (byteArray[1] << 8) | (byteArray[2] << 16);
							floats[i] = intSample24 / (float) (1 << 23);
							break;
						case 32:
							int intSample32 = BitConverter.ToInt32(byteArray, 0);
							floats[i] = intSample32 / (float) int.MaxValue;
							break;
						default:
							throw new ArgumentException("Unsupported bit depth");
					}
				}

				// Return floats
				return floats;
			}

			public Bitmap DrawWaveformSmooth(PictureBox wavebox, long offset = 0, int samplesPerPixel = 1, bool update = false, Color? graph = null)
			{
				// Überprüfen, ob floats und die PictureBox gültig sind
				if (Data.Length == 0 || wavebox.Width <= 0 || wavebox.Height <= 0)
				{
					// Empty picturebox
					if (update)
					{
						wavebox.Image = null;
						wavebox.Refresh();
					}

					return new Bitmap(1, 1);
				}

				// Colors (background depends on graph brightness)
				Color waveformColor = graph ?? Color.FromName("HotTrack");
				Color backgroundColor = waveformColor.GetBrightness() < 0.5 ? Color.White : Color.Black;


				Bitmap bmp = new(wavebox.Width, wavebox.Height);
				using Graphics gfx = Graphics.FromImage(bmp);
				using Pen pen = new(waveformColor);
				gfx.SmoothingMode = SmoothingMode.AntiAlias;
				gfx.Clear(backgroundColor);

				float centerY = wavebox.Height / 2f;
				float yScale = wavebox.Height / 2f;

				for (int x = 0; x < wavebox.Width; x++)
				{
					long sampleIndex = offset + (long) x * samplesPerPixel;

					if (sampleIndex >= Data.Length)
					{
						break;
					}

					float maxValue = float.MinValue;
					float minValue = float.MaxValue;

					for (int i = 0; i < samplesPerPixel; i++)
					{
						if (sampleIndex + i < Data.Length)
						{
							maxValue = Math.Max(maxValue, Data[sampleIndex + i]);
							minValue = Math.Min(minValue, Data[sampleIndex + i]);
						}
					}

					float yMax = centerY - maxValue * yScale;
					float yMin = centerY - minValue * yScale;

					// Überprüfen, ob die Werte innerhalb des sichtbaren Bereichs liegen
					if (yMax < 0) yMax = 0;
					if (yMin > wavebox.Height) yMin = wavebox.Height;

					// Zeichne die Linie nur, wenn sie sichtbar ist
					if (Math.Abs(yMax - yMin) > 0.01f)
					{
						gfx.DrawLine(pen, x, yMax, x, yMin);
					}
					else if (samplesPerPixel == 1)
					{
						// Zeichne einen Punkt, wenn samplesPerPixel 1 ist und die Linie zu klein ist
						gfx.DrawLine(pen, x, centerY, x, centerY - Data[sampleIndex] * yScale);
					}
				}

				// Update PictureBox
				if (update)
				{
					wavebox.Image = bmp;
					wavebox.Refresh();
				}

				return bmp;
			}

			public int GetFitResolution(int width)
			{
				// Gets pixels per sample for a given width to fit the whole waveform
				int samplesPerPixel = (int) Math.Ceiling((double) Data.Length / width) / 4;
				return samplesPerPixel;
			}

			public byte[] AddWaveheader(bool update = false)
			{
				// Abort if no data or data starts with waveheader
				if (Data.Length == 0 || Data[0] == 0x52 && Data[1] == 0x49 && Data[2] == 0x46 && Data[3] == 0x46)
				{
					return [];
				}

				// Create header data
				byte[] header = new byte[44];

				// RIFF header
				header[0] = 0x52; // R
				header[1] = 0x49; // I
				header[2] = 0x46; // F
				header[3] = 0x46; // F

				// File size
				byte[] fileSize = BitConverter.GetBytes(Data.Length + 36);
				Buffer.BlockCopy(fileSize, 0, header, 4, 4);

				// WAVE header
				header[8] = 0x57; // W
				header[9] = 0x41; // A
				header[10] = 0x56; // V
				header[11] = 0x45; // E

				// FMT chunk
				header[12] = 0x66; // f
				header[13] = 0x6D; // m
				header[14] = 0x74; // t
				header[15] = 0x20; // space

				// Subchunk size
				byte[] subchunkSize = BitConverter.GetBytes(16);
				Buffer.BlockCopy(subchunkSize, 0, header, 16, 4);

				// Audio format
				byte[] audioFormat = BitConverter.GetBytes(1);
				Buffer.BlockCopy(audioFormat, 0, header, 20, 2);

				// Number of channels
				byte[] numChannels = BitConverter.GetBytes(Channels);
				Buffer.BlockCopy(numChannels, 0, header, 22, 2);

				// Sample rate
				byte[] sampleRate = BitConverter.GetBytes(Samplerate);
				Buffer.BlockCopy(sampleRate, 0, header, 24, 4);

				// Byte rate
				byte[] byteRate = BitConverter.GetBytes(Samplerate * Channels * Bitdepth / 8);
				Buffer.BlockCopy(byteRate, 0, header, 28, 4);

				// Block align
				byte[] blockAlign = BitConverter.GetBytes(Channels * Bitdepth / 8);
				Buffer.BlockCopy(blockAlign, 0, header, 32, 2);

				// Bits per sample
				byte[] bitsPerSample = BitConverter.GetBytes(Bitdepth);
				Buffer.BlockCopy(bitsPerSample, 0, header, 34, 2);

				// Data chunk
				header[36] = 0x64; // d
				header[37] = 0x61; // a
				header[38] = 0x74; // t
				header[39] = 0x61; // a

				// Data size
				byte[] dataSize = BitConverter.GetBytes(Data.Length);
				Buffer.BlockCopy(dataSize, 0, header, 40, 4);

				// Get data bytes
				byte[] dataBytes = GetBytes();
				long length = dataBytes.Length - 1;

				// Add header to data
				byte[] newData = new byte[length + 44];
				Buffer.BlockCopy(header, 0, newData, 0, 44);
				Buffer.BlockCopy(dataBytes, 0, newData, 44, (int) length);

				// If update: Set new data
				if (update)
				{
					Data = GetFloats(newData);
				}

				// Return new data
				return newData;
			}

			public void Normalize(float factor = 1.0f)
			{
				long length = Data.LongLength;

				if (factor == 1.0f)
				{
					Normalize();
					return;
				}

				if (length == 0)
				{
					return;
				}

				for (int i = 0; i < length; i++)
				{
					Data[i] /= (length * factor);
				}
			}

			public void Normalize()
			{
				// Normalize data by dividing by count of samples
				for (int i = 0; i < Data.Length; i++)
				{
					Data[i] /= Data.Length;
				}
			}

			public Tuple<AudioObject, AudioObject?> Split(bool ignoreChannels = false)
			{
				// Abort if no data
				if (Data.Length == 0)
				{
					// Return tuple
					return new Tuple<AudioObject, AudioObject?>(this, null);
				}

				// If Channels != 2 and ignoreChannels is false, return tuple
				if (Channels != 2 && !ignoreChannels)
				{
					// Return tuple
					return new Tuple<AudioObject, AudioObject?>(this, null);
				}

				// Create new tracks for left and right channel
				AudioObject left = new("")
				{
					Data = new float[Data.Length / 2],
					Name = "(L) " + Name,
					Samplerate = Samplerate,
					Bitdepth = Bitdepth,
					Channels = 1,
					Length = Data.Length / 2,
					Duration = Data.Length / 2 / Samplerate
				};

				AudioObject right = new("")
				{
					Data = new float[Data.Length / 2],
					Name = "(R) " + Name,
					Samplerate = Samplerate,
					Bitdepth = Bitdepth,
					Channels = 1,
					Length = Data.Length / 2,
					Duration = Data.Length / 2 / Samplerate
				};

				// Split data
				for (int i = 0; i < Data.Length; i += 2)
				{
					left.Data[i / 2] = Data[i];
					right.Data[i / 2] = Data[i + 1];
				}

				// Return tuple
				return new Tuple<AudioObject, AudioObject?>(left, right);
			}

			public void Play(Button? b = null)
			{
				// Abort if no data or already playing
				if (Data.Length == 0 || Player.PlaybackState == PlaybackState.Playing)
				{
					// Set button text
					if (b != null)
					{
						b.Text = @"▶";
					}

					return;
				}

				// Create waveformat
				WaveFormat format = new(Samplerate, Bitdepth, Channels);

				// Create memory stream with format
				MemoryStream stream = new(GetBytes());
				WaveStream waveStream = new RawSourceWaveStream(stream, format);

				// Set button text
				if (b != null)
				{
					b.Text = @"⏹";
				}

				// Set player
				Player.Init(waveStream);
				Player.Play();
			}

			public void Stop(Button? b = null)
			{
				// Abort if not playing
				if (Player.PlaybackState != PlaybackState.Playing)
				{
					// Set button text
					if (b != null)
					{
						b.Text = @"▶";
					}
					return;
				}

				// Stop player
				Player.Stop();

				// Set button text
				if (b != null)
				{
					b.Text = @"▶";
				}
			}

			public void PlayStop(Button? b = null)
			{
				// Play if not playing, stop if playing
				if (Player.PlaybackState != PlaybackState.Playing)
				{
					// Set button text
					if (b != null)
					{
						b.Text = @"⏹";
					}

					Play();
				}
				else
				{
					// Set button text
					if (b != null)
					{
						b.Text = @"▶";
					}

					Stop();
				}
			}

			public long GetCurrentSample()
			{
				// Get current sample
				long sample = 0;

				// If playing, get position
				if (Player.PlaybackState == PlaybackState.Playing)
				{
					sample = Player.GetPosition() / (Bitdepth / 8);
				}

				return sample;
			}

			public string GetCurrentTimestamp()
			{
				// Get current timestamp
				long sample = GetCurrentSample();
				double time = sample / (double) Samplerate;

				return TimeSpan.FromSeconds(time).ToString(@"mm\:ss\.fff");
			}

			public bool IsPlaying()
			{
				// Return if player is playing
				return Player.PlaybackState == PlaybackState.Playing;
			}


		}
	}
}
