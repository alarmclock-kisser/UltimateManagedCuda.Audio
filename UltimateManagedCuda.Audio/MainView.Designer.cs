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
			((System.ComponentModel.ISupportInitialize) pictureBox_waveform).BeginInit();
			SuspendLayout();
			// 
			// listBox_tracks
			// 
			listBox_tracks.FormattingEnabled = true;
			listBox_tracks.ItemHeight = 15;
			listBox_tracks.Location = new Point(12, 466);
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
			listBox_pointers.Location = new Point(412, 466);
			listBox_pointers.Name = "listBox_pointers";
			listBox_pointers.Size = new Size(280, 154);
			listBox_pointers.TabIndex = 4;
			// 
			// pictureBox_waveform
			// 
			pictureBox_waveform.BackColor = Color.White;
			pictureBox_waveform.Location = new Point(12, 340);
			pictureBox_waveform.Name = "pictureBox_waveform";
			pictureBox_waveform.Size = new Size(680, 120);
			pictureBox_waveform.TabIndex = 5;
			pictureBox_waveform.TabStop = false;
			// 
			// button_color
			// 
			button_color.BackColor = SystemColors.HotTrack;
			button_color.ForeColor = Color.White;
			button_color.Location = new Point(617, 12);
			button_color.Name = "button_color";
			button_color.Size = new Size(75, 23);
			button_color.TabIndex = 6;
			button_color.Text = "Color";
			button_color.UseVisualStyleBackColor = false;
			button_color.Click += button_color_Click;
			// 
			// label_trackMeta
			// 
			label_trackMeta.AutoSize = true;
			label_trackMeta.BackColor = Color.Transparent;
			label_trackMeta.Location = new Point(12, 322);
			label_trackMeta.Name = "label_trackMeta";
			label_trackMeta.Size = new Size(91, 15);
			label_trackMeta.TabIndex = 7;
			label_trackMeta.Text = "Track meta data";
			// 
			// label_tracksInfo
			// 
			label_tracksInfo.AutoSize = true;
			label_tracksInfo.Location = new Point(12, 623);
			label_tracksInfo.Name = "label_tracksInfo";
			label_tracksInfo.Size = new Size(57, 15);
			label_tracksInfo.TabIndex = 8;
			label_tracksInfo.Text = "Tracks (0)";
			// 
			// label_pointersInfo
			// 
			label_pointersInfo.AutoSize = true;
			label_pointersInfo.Location = new Point(412, 623);
			label_pointersInfo.Name = "label_pointersInfo";
			label_pointersInfo.Size = new Size(67, 15);
			label_pointersInfo.TabIndex = 9;
			label_pointersInfo.Text = "Pointers (0)";
			// 
			// MainView
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(704, 681);
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
	}
}
