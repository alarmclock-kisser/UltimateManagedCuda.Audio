namespace UltimateManagedCuda.Audio
{
	public partial class MainView : Form
	{
		// ~~~~~ ~~~~~ ~~~~~ ATTRIBUTES ~~~~~ ~~~~~ ~~~~~ \\
		public AudioHandling AudioH;
		public CudaHandling CudaH;

		public int Fps = 120;
		public System.Windows.Forms.Timer PlaybackTimer;

		public long Offset = 0;
		public int SamplesPerPixel = 1024;




		// ~~~~~ ~~~~~ ~~~~~ CONSTRUCTORS ~~~~~ ~~~~~ ~~~~~ \\
		public MainView()
		{
			InitializeComponent();

			// Window position
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(0, 0);

			// Init. classes
			AudioH = new AudioHandling(listBox_tracks, label_tracksInfo);
			CudaH = new CudaHandling(comboBox_cudaDevice, label_cudaVram, progressBar_vramUsage, listBox_pointers, label_pointersInfo);

			// Register events
			listBox_tracks.Click += RemoveTrack;
			listBox_tracks.DoubleClick += MoveTrack;
			listBox_pointers.Click += ClearPointer;
			listBox_pointers.DoubleClick += PerformFFT;
			pictureBox_waveform.MouseWheel += ScrollZoom;

			// Init. playback timer
			PlaybackTimer = new System.Windows.Forms.Timer();
			PlaybackTimer.Interval = 1000 / Fps;
			PlaybackTimer.Tick += listBox_tracks_SelectedIndexChanged;

			// Init. CUDA device (or Host)
			comboBox_cudaDevice.SelectedIndex = 0;
		}





		// ~~~~~ ~~~~~ ~~~~~ METHODS ~~~~~ ~~~~~ ~~~~~ \\




		// ~~~~~ ~~~~~ ~~~~~ EVENTS ~~~~~ ~~~~~ ~~~~~ \\
		// ~~~~~ Audio ~~~~~ \\
		public void ImportTrack(object? sender, EventArgs? e)
		{
			// Open file dialog
			OpenFileDialog dialog = new()
			{
				Title = "Select audio file",
				Filter = "Audio files|*.wav;*.mp3;*.flac",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				Multiselect = true,
				CheckFileExists = true
			};

			// OFD show -> AudioH.AddTrack
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				foreach (var file in dialog.FileNames)
				{
					AudioH.AddTrack(file);
				}
			}
		}

		private void button_import_Click(object sender, EventArgs e)
		{
			// Import track
			ImportTrack(null, null);
		}

		public void RemoveTrack(object? sender, EventArgs e)
		{
			// Abort if not CTRL + Click
			if (ModifierKeys != Keys.Control)
			{
				return;
			}

			// Get track
			int index = listBox_tracks.SelectedIndex;

			// If no track is selected, abort
			if (index == -1)
			{
				return;
			}

			// If not SHIFT down: Remove & clear pointers
			if (ModifierKeys != Keys.Shift)
			{
				CudaH.ClearPointers(AudioH.Tracks[index].Pointer, AudioH.Tracks[index].Spectrum);
			}

			// Remove track
			AudioH.RemoveTrack(index);

			// GC
			GC.Collect();

			// Update GUI
			AudioH.UpdateTracksList();
		}

		private void listBox_tracks_SelectedIndexChanged(object? sender, EventArgs? e)
		{
			// Get track
			int index = listBox_tracks.SelectedIndex;

			// If no track is selected, abort
			if (index == -1)
			{
				label_trackMeta.Text = "No track selected.";
				pictureBox_waveform.Image = null;
				return;
			}

			// If data != [], draw waveform
			if (AudioH.Tracks[index].Data.Length > 0)
			{
				// Get offset
				if (AudioH.Tracks[index].IsPlaying())
				{
					Offset = AudioH.Tracks[index].GetCurrentSample();
				}

				// Get resolution
				int res = SamplesPerPixel;
				if (Offset == 0)
				{
					res = AudioH.Tracks[index].GetFitResolution(pictureBox_waveform.Width);
				}

				// Draw waveform
				AudioH.Tracks[index].DrawWaveformSmooth(pictureBox_waveform, Offset, res, true, button_color.BackColor);
			}
			else
			{
				pictureBox_waveform.Image = null;
			}

			// Update meta
			int samplerate = AudioH.Tracks[index].Samplerate;
			int bitdepth = AudioH.Tracks[index].Bitdepth;
			int channels = AudioH.Tracks[index].Channels;
			long length = AudioH.Tracks[index].Length;
			double duration = AudioH.Tracks[index].Duration;
			label_trackMeta.Text = $"{samplerate} Hz, {bitdepth} bit, {channels} Ch., {length} f32, {duration:0.00.000} sec.";
		}

		private void button_color_Click(object sender, EventArgs e)
		{
			// Open color dialog
			ColorDialog dialog = new()
			{
				AllowFullOpen = true,
				AnyColor = true,
				FullOpen = true,
				Color = button_color.BackColor
			};

			// CD show -> Set color
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				button_color.BackColor = dialog.Color;

				// Adjust foreground color
				if (dialog.Color.GetBrightness() < 0.5)
				{
					button_color.ForeColor = Color.White;

				}
				else
				{
					button_color.ForeColor = Color.Black;
				}
			}

			// Update waveform
			listBox_tracks_SelectedIndexChanged(null, null);
		}

		private void button_splitTrack_Click(object sender, EventArgs e)
		{
			// Get track
			int index = listBox_tracks.SelectedIndex;

			// If no track is selected, abort
			if (index == -1)
			{
				return;
			}

			// Variable for two tracks
			Tuple<AudioHandling.AudioObject, AudioHandling.AudioObject?> tracks;

			// Split track (ignore channels if CTRL down)
			if (ModifierKeys == Keys.Control)
			{
				tracks = AudioH.Tracks[index].Split(true);
			}
			else
			{
				tracks = AudioH.Tracks[index].Split();
			}

			// Remove old track
			AudioH.RemoveTrack(index);

			// Add new tracks
			AudioH.Tracks.Add(tracks.Item1);
			if (tracks.Item2 != null)
			{
				AudioH.Tracks.Add(tracks.Item2);
			}

			// Update GUI
			AudioH.UpdateTracksList();
		}

		private void ScrollZoom(object? sender, MouseEventArgs e)
		{
			// Get track
			int index = listBox_tracks.SelectedIndex;

			// If no track is selected, abort
			if (index == -1)
			{
				return;
			}

			// If CTRL down: Zoom
			if (ModifierKeys == Keys.Control)
			{
				// Zoom in
				if (e.Delta > 0)
				{
					SamplesPerPixel = Math.Max(1, (int) (SamplesPerPixel / 1.5f));
				}
				// Zoom out
				else
				{
					// Help when value is 1
					if (SamplesPerPixel == 1)
					{
						SamplesPerPixel = 2;
					}

					SamplesPerPixel = Math.Min(20000, (int) (SamplesPerPixel * 1.5f));
				}
			}

			// Else: Scroll
			else
			{
				// Scroll waveform
				if (e.Delta > 0)
				{
					Offset = Math.Max(0, Offset - SamplesPerPixel * 25);
				}
				else
				{
					Offset = Math.Min(AudioH.Tracks[index].Data.Length - SamplesPerPixel, Offset + SamplesPerPixel * 25);
				}
			}

			// Update waveform afterwards
			listBox_tracks_SelectedIndexChanged(null, null);

		}

		private void numericUpDown_fps_ValueChanged(object sender, EventArgs e)
		{
			// Update FPS
			Fps = (int) numericUpDown_fps.Value;

			// Update timer
			PlaybackTimer.Interval = 1000 / Fps;
		}



		// ~~~~~ Playback ~~~~~ \\


		// ~~~~~ CUDA ~~~~~ \\
		private void comboBox_cudaDevice_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Get device ID
			int id = comboBox_cudaDevice.SelectedIndex;

			// Init. device
			CudaH?.InitDevice(id);
		}

		private void MoveTrack(object? sender, EventArgs? e)
		{
			// Get track
			int track = listBox_tracks.SelectedIndex;

			// If no track is selected, abort
			if (track == -1)
			{
				return;
			}

			// To Cuda if data != []
			if (AudioH.Tracks[track].Data.Length > 0)
			{
				// Move track to CUDA -> track.Pointer
				var ptr = CudaH?.PushToCuda(AudioH.Tracks[track])?.Pointer ?? 0;

				// If successful, clear track data
				if (ptr != 0)
				{
					AudioH.Tracks[track].Pointer = ptr;
					AudioH.Tracks[track].Data = [];
				}
			}
			else if (AudioH.Tracks[track].Pointer != 0)
			{
				// Move track from CUDA -> track.Data
				var data = CudaH?.PullFromCuda(AudioH.Tracks[track].Pointer) ?? [];

				// If successful, clear track pointer
				if (data.Length > 0)
				{
					AudioH.Tracks[track].Data = data;
					AudioH.Tracks[track].Pointer = 0;
				}
			}

			// Update GUI
			AudioH.UpdateTracksList();
			listBox_tracks_SelectedIndexChanged(null, null);
		}

		private void ClearPointer(object? sender, EventArgs e)
		{
			// Abort if not CTRL + Click
			if (ModifierKeys != Keys.Control)
			{
				return;
			}

			// Get pointer
			int index = listBox_pointers.SelectedIndex;

			// If no pointer is selected, abort
			if (index == -1 || CudaH.Ctx == null)
			{
				return;
			}

			// Get pointer
			var ptr = CudaH.Pointers.ElementAt(index).Key;

			// Free memory
			long freed = CudaH.ClearPointer(0, ptr, true);

			// If successful, remove pointer from track
			if (freed > 0)
			{
				AudioH.RemoveTrack((IntPtr) ptr.Pointer);
			}

			// Update GUI
			CudaH.UpdatePointersList();
		}

		private void PerformFFT(object? sender, EventArgs e)
		{
			// Get pointer
			int index = listBox_pointers.SelectedIndex;

			// If no pointer is selected, abort
			if (index == -1 || CudaH.Ctx == null)
			{
				return;
			}

			// Get FFT direction from <char> & pointer attributes
			var ptr = CudaH.Pointers.ElementAt(index).Key;
			char type = CudaH.Pointers.ElementAt(index).Value.Item2;
			int samplerate = CudaH.Pointers.ElementAt(index).Value.Item3;
			int bitdepth = CudaH.Pointers.ElementAt(index).Value.Item4;
			int channels = CudaH.Pointers.ElementAt(index).Value.Item5;
			string name = CudaH.Pointers.ElementAt(index).Value.Item6;

			// If type is 'd': FFT -> Inverse FFT
			if (type == 'd')
			{
				// Perform FFT forward
				CudaH.PerformForwardFFT(0, ptr);
			}
			else if (type == 'c')
			{
				// Perform Inverse FFT if type is 'c'
				CudaH.PerformInverseFFT(0, ptr);
			}
			else if (type == 'n')
			{
				// Make track if type is 'n'
				index = AudioH.MakeTrack(CudaH.PullFromCuda(0, ptr), name, samplerate, bitdepth, channels);

				// Normalize track
				AudioH.Tracks[index].Normalize(1.0f);
			}
			else
			{
				// Handle the case where the track is not found
				MessageBox.Show("Track not found for the given pointer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void button_playStop_Click(object sender, EventArgs e)
		{
			// Get track
			int index = listBox_tracks.SelectedIndex;

			// If no track is selected, abort
			if (index == -1)
			{
				return;
			}

			// Play or stop track
			if (AudioH.Tracks[index].IsPlaying())
			{
				PlaybackTimer.Stop();
				AudioH.Tracks[index].Stop(button_playStop);
			}
			else
			{
				PlaybackTimer.Start();
				AudioH.Tracks[index].Play(button_playStop);
			}
		}

		private void button_stretchFactor_Click(object sender, EventArgs e)
		{
			// Get track
			int index = listBox_tracks.SelectedIndex;

			// If no track is selected, abort
			if (index == -1)
			{
				return;
			}

			// If no Pointer: Push to CUDA
			if (AudioH.Tracks[index].Pointer == 0)
			{
				MoveTrack(null, null);
			}

			// Get pointer from CudaH.Pointers
			var ptr = CudaH.Pointers.ElementAt(CudaH.GetPointerIndex(AudioH.Tracks[index].Pointer)).Key;

			// Get stretch factor
			float factor = (float) numericUpDown_factor.Value;

			// FFT -> Stretch -> Inverse FFT
			var fftPointer = CudaH.PerformForwardFFT(0, ptr);

			// Stretch
			fftPointer = CudaH.StretchPointerData(0, fftPointer, factor);

			// Inverse FFT
			var invPointer = CudaH.PerformInverseFFT(0, fftPointer);

			// Make track
			index = AudioH.MakeTrack(CudaH.PullFromCuda(0, invPointer), "Stretched <" + (invPointer?.Pointer.ToString() ?? "") + ">", AudioH.Tracks[index].Samplerate, AudioH.Tracks[index].Bitdepth, AudioH.Tracks[index].Channels);

			// Normalize track
			AudioH.Tracks[index].Normalize(factor);

			// Update GUI
			AudioH.UpdateTracksList();
			CudaH.UpdatePointersList();
		}
	}
}
