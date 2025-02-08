namespace UltimateManagedCuda.Audio
{
    partial class MainView
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			listBox_tracks = new ListBox();
			comboBox_cudaDevice = new ComboBox();
			label_cudaVram = new Label();
			progressBar_vramUsage = new ProgressBar();
			listBox_pointers = new ListBox();
			pictureBox_waveform = new PictureBox();
			button_color = new Button();
			label_trackMeta = new Label();
			label_tracksInfo = new Label();
			label_pointersInfo = new Label();
			button_splitTrack = new Button();
			button_import = new Button();
			button_playStop = new Button();
			numericUpDown_fps = new NumericUpDown();
			label_fps = new Label();
			button_stretchFactor = new Button();
			numericUpDown_factor = new NumericUpDown();
			label_factor = new Label();
			groupBox_transformations = new GroupBox();
			button_transAddKernel = new Button();
			button_transFftInv = new Button();
			button_transFftFwd = new Button();
			listBox_kernels = new ListBox();
			((System.ComponentModel.ISupportInitialize) pictureBox_waveform).BeginInit();
			((System.ComponentModel.ISupportInitialize) numericUpDown_fps).BeginInit();
			((System.ComponentModel.ISupportInitialize) numericUpDown_factor).BeginInit();
			groupBox_transformations.SuspendLayout();
			SuspendLayout();
			// 
			// listBox_tracks
			// 
			listBox_tracks.FormattingEnabled = true;
			listBox_tracks.ItemHeight = 15;
			listBox_tracks.Location = new Point(12, 485);
			listBox_tracks.Name = "listBox_tracks";
			listBox_tracks.Size = new Size(280, 154);
			listBox_tracks.TabIndex = 0;
			listBox_tracks.SelectedIndexChanged += listBox_tracks_SelectedIndexChanged;
			// 
			// comboBox_cudaDevice
			// 
			comboBox_cudaDevice.FormattingEnabled = true;
			comboBox_cudaDevice.Location = new Point(12, 12);
			comboBox_cudaDevice.Name = "comboBox_cudaDevice";
			comboBox_cudaDevice.Size = new Size(280, 23);
			comboBox_cudaDevice.TabIndex = 1;
			comboBox_cudaDevice.Text = "Select CUDA device to initialize context";
			comboBox_cudaDevice.SelectedIndexChanged += comboBox_cudaDevice_SelectedIndexChanged;
			// 
			// label_cudaVram
			// 
			label_cudaVram.AutoSize = true;
			label_cudaVram.Location = new Point(12, 39);
			label_cudaVram.Name = "label_cudaVram";
			label_cudaVram.Size = new Size(90, 15);
			label_cudaVram.TabIndex = 2;
			label_cudaVram.Text = "VRAM: 0 / 0 MB";
			// 
			// progressBar_vramUsage
			// 
			progressBar_vramUsage.Location = new Point(12, 56);
			progressBar_vramUsage.Name = "progressBar_vramUsage";
			progressBar_vramUsage.Size = new Size(280, 10);
			progressBar_vramUsage.TabIndex = 3;
			// 
			// listBox_pointers
			// 
			listBox_pointers.FormattingEnabled = true;
			listBox_pointers.ItemHeight = 15;
			listBox_pointers.Location = new Point(412, 485);
			listBox_pointers.Name = "listBox_pointers";
			listBox_pointers.Size = new Size(280, 154);
			listBox_pointers.TabIndex = 4;
			// 
			// pictureBox_waveform
			// 
			pictureBox_waveform.BackColor = Color.White;
			pictureBox_waveform.Location = new Point(12, 359);
			pictureBox_waveform.Name = "pictureBox_waveform";
			pictureBox_waveform.Size = new Size(680, 120);
			pictureBox_waveform.TabIndex = 5;
			pictureBox_waveform.TabStop = false;
			// 
			// button_color
			// 
			button_color.BackColor = SystemColors.HotTrack;
			button_color.ForeColor = Color.White;
			button_color.Location = new Point(612, 12);
			button_color.Name = "button_color";
			button_color.Size = new Size(80, 23);
			button_color.TabIndex = 6;
			button_color.Text = "Color";
			button_color.UseVisualStyleBackColor = false;
			button_color.Click += button_color_Click;
			// 
			// label_trackMeta
			// 
			label_trackMeta.AutoSize = true;
			label_trackMeta.BackColor = Color.Transparent;
			label_trackMeta.Location = new Point(12, 341);
			label_trackMeta.Name = "label_trackMeta";
			label_trackMeta.Size = new Size(91, 15);
			label_trackMeta.TabIndex = 7;
			label_trackMeta.Text = "Track meta data";
			// 
			// label_tracksInfo
			// 
			label_tracksInfo.AutoSize = true;
			label_tracksInfo.Location = new Point(12, 642);
			label_tracksInfo.Name = "label_tracksInfo";
			label_tracksInfo.Size = new Size(57, 15);
			label_tracksInfo.TabIndex = 8;
			label_tracksInfo.Text = "Tracks (0)";
			// 
			// label_pointersInfo
			// 
			label_pointersInfo.AutoSize = true;
			label_pointersInfo.Location = new Point(412, 642);
			label_pointersInfo.Name = "label_pointersInfo";
			label_pointersInfo.Size = new Size(67, 15);
			label_pointersInfo.TabIndex = 9;
			label_pointersInfo.Text = "Pointers (0)";
			// 
			// button_splitTrack
			// 
			button_splitTrack.Location = new Point(298, 616);
			button_splitTrack.Name = "button_splitTrack";
			button_splitTrack.Size = new Size(60, 23);
			button_splitTrack.TabIndex = 10;
			button_splitTrack.Text = "Split L/R";
			button_splitTrack.UseVisualStyleBackColor = true;
			button_splitTrack.Click += button_splitTrack_Click;
			// 
			// button_import
			// 
			button_import.Location = new Point(187, 645);
			button_import.Name = "button_import";
			button_import.Size = new Size(75, 24);
			button_import.TabIndex = 11;
			button_import.Text = "Import";
			button_import.UseVisualStyleBackColor = true;
			button_import.Click += button_import_Click;
			// 
			// button_playStop
			// 
			button_playStop.Location = new Point(268, 645);
			button_playStop.Name = "button_playStop";
			button_playStop.Size = new Size(24, 24);
			button_playStop.TabIndex = 12;
			button_playStop.Text = "▶";
			button_playStop.UseVisualStyleBackColor = true;
			button_playStop.Click += button_playStop_Click;
			// 
			// numericUpDown_fps
			// 
			numericUpDown_fps.Location = new Point(647, 41);
			numericUpDown_fps.Maximum = new decimal(new int[] { 240, 0, 0, 0 });
			numericUpDown_fps.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numericUpDown_fps.Name = "numericUpDown_fps";
			numericUpDown_fps.Size = new Size(45, 23);
			numericUpDown_fps.TabIndex = 13;
			numericUpDown_fps.Value = new decimal(new int[] { 120, 0, 0, 0 });
			numericUpDown_fps.ValueChanged += numericUpDown_fps_ValueChanged;
			// 
			// label_fps
			// 
			label_fps.AutoSize = true;
			label_fps.Location = new Point(612, 43);
			label_fps.Name = "label_fps";
			label_fps.Size = new Size(29, 15);
			label_fps.TabIndex = 14;
			label_fps.Text = "FPS:";
			// 
			// button_stretchFactor
			// 
			button_stretchFactor.Location = new Point(346, 485);
			button_stretchFactor.Name = "button_stretchFactor";
			button_stretchFactor.Size = new Size(60, 23);
			button_stretchFactor.TabIndex = 15;
			button_stretchFactor.Text = "Stretch";
			button_stretchFactor.UseVisualStyleBackColor = true;
			button_stretchFactor.Click += button_stretchFactor_Click;
			// 
			// numericUpDown_factor
			// 
			numericUpDown_factor.DecimalPlaces = 10;
			numericUpDown_factor.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
			numericUpDown_factor.Location = new Point(298, 514);
			numericUpDown_factor.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDown_factor.Minimum = new decimal(new int[] { 5, 0, 0, 131072 });
			numericUpDown_factor.Name = "numericUpDown_factor";
			numericUpDown_factor.Size = new Size(108, 23);
			numericUpDown_factor.TabIndex = 16;
			numericUpDown_factor.Value = new decimal(new int[] { 10, 0, 0, 65536 });
			// 
			// label_factor
			// 
			label_factor.AutoSize = true;
			label_factor.Location = new Point(298, 493);
			label_factor.Name = "label_factor";
			label_factor.Size = new Size(40, 15);
			label_factor.TabIndex = 17;
			label_factor.Text = "Factor";
			// 
			// groupBox_transformations
			// 
			groupBox_transformations.Controls.Add(button_transAddKernel);
			groupBox_transformations.Controls.Add(button_transFftInv);
			groupBox_transformations.Controls.Add(button_transFftFwd);
			groupBox_transformations.Location = new Point(412, 199);
			groupBox_transformations.Name = "groupBox_transformations";
			groupBox_transformations.Size = new Size(280, 154);
			groupBox_transformations.TabIndex = 18;
			groupBox_transformations.TabStop = false;
			groupBox_transformations.Text = "Transformations (CUDA)";
			// 
			// button_transAddKernel
			// 
			button_transAddKernel.Location = new Point(199, 22);
			button_transAddKernel.Name = "button_transAddKernel";
			button_transAddKernel.Size = new Size(75, 23);
			button_transAddKernel.TabIndex = 19;
			button_transAddKernel.Text = "+ Kernel";
			button_transAddKernel.UseVisualStyleBackColor = true;
			button_transAddKernel.Click += button_transAddKernel_Click;
			// 
			// button_transFftInv
			// 
			button_transFftInv.Location = new Point(6, 51);
			button_transFftInv.Name = "button_transFftInv";
			button_transFftInv.Size = new Size(75, 23);
			button_transFftInv.TabIndex = 19;
			button_transFftInv.Text = "FFT INV";
			button_transFftInv.UseVisualStyleBackColor = true;
			// 
			// button_transFftFwd
			// 
			button_transFftFwd.Location = new Point(6, 22);
			button_transFftFwd.Name = "button_transFftFwd";
			button_transFftFwd.Size = new Size(75, 23);
			button_transFftFwd.TabIndex = 19;
			button_transFftFwd.Text = "FFT FWD";
			button_transFftFwd.UseVisualStyleBackColor = true;
			// 
			// listBox_kernels
			// 
			listBox_kernels.FormattingEnabled = true;
			listBox_kernels.ItemHeight = 15;
			listBox_kernels.Location = new Point(298, 214);
			listBox_kernels.Name = "listBox_kernels";
			listBox_kernels.Size = new Size(108, 139);
			listBox_kernels.TabIndex = 19;
			// 
			// MainView
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(704, 681);
			Controls.Add(listBox_kernels);
			Controls.Add(groupBox_transformations);
			Controls.Add(label_factor);
			Controls.Add(numericUpDown_factor);
			Controls.Add(button_stretchFactor);
			Controls.Add(label_fps);
			Controls.Add(numericUpDown_fps);
			Controls.Add(button_playStop);
			Controls.Add(button_import);
			Controls.Add(button_splitTrack);
			Controls.Add(label_pointersInfo);
			Controls.Add(label_tracksInfo);
			Controls.Add(label_trackMeta);
			Controls.Add(button_color);
			Controls.Add(pictureBox_waveform);
			Controls.Add(listBox_pointers);
			Controls.Add(progressBar_vramUsage);
			Controls.Add(label_cudaVram);
			Controls.Add(comboBox_cudaDevice);
			Controls.Add(listBox_tracks);
			MaximizeBox = false;
			MaximumSize = new Size(720, 720);
			MinimumSize = new Size(720, 720);
			Name = "MainView";
			Text = "ULTIMATE ManagedCuda-12 (Audio)";
			((System.ComponentModel.ISupportInitialize) pictureBox_waveform).EndInit();
			((System.ComponentModel.ISupportInitialize) numericUpDown_fps).EndInit();
			((System.ComponentModel.ISupportInitialize) numericUpDown_factor).EndInit();
			groupBox_transformations.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ListBox listBox_tracks;
		private ComboBox comboBox_cudaDevice;
		private Label label_cudaVram;
		private ProgressBar progressBar_vramUsage;
		private ListBox listBox_pointers;
		private PictureBox pictureBox_waveform;
		private Button button_color;
		private Label label_trackMeta;
		private Label label_tracksInfo;
		private Label label_pointersInfo;
		private Button button_splitTrack;
		private Button button_import;
		private Button button_playStop;
		private NumericUpDown numericUpDown_fps;
		private Label label_fps;
		private Button button_stretchFactor;
		private NumericUpDown numericUpDown_factor;
		private Label label_factor;
		private GroupBox groupBox_transformations;
		private Button button_transAddKernel;
		private Button button_transFftInv;
		private Button button_transFftFwd;
		private ListBox listBox_kernels;
	}
}
