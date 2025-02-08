using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaFFT;
using ManagedCuda.VectorTypes;
using System.Reflection;
using static UltimateManagedCuda.Audio.AudioHandling;

namespace UltimateManagedCuda.Audio
{
	public class CudaHandling
	{
		// ~~~~~ ~~~~~ ~~~~~ ATTRIBUTES ~~~~~ ~~~~~ ~~~~~ \\
		public ComboBox? DevicesBox;
		public Label? VramInfoLabel;
		public ProgressBar? VramInfoBar;
		public ListBox? PointersList;
		public Label? PointersInfoLabel;

		public int DeviceId = -1;

		public PrimaryContext? Ctx = null;

		public Dictionary<CUdeviceptr, Tuple<long, char, int, int, int, string>> Pointers = [];




		// ~~~~~ ~~~~~ ~~~~~ CONSTRUCTORS ~~~~~ ~~~~~ ~~~~~ \\
		public CudaHandling(ComboBox? devicesBox = null, Label? vramInfo = null, ProgressBar? vramBar = null, ListBox? pointersList = null, Label? pointersInfo = null)
		{
			// Set objects
			DevicesBox = devicesBox;
			VramInfoLabel = vramInfo;
			VramInfoBar = vramBar;
			PointersList = pointersList;
			PointersInfoLabel = pointersInfo;

			// Update GUI
			UpdateDevicesBox(false);
		}





		// ~~~~~ ~~~~~ ~~~~~ METHODS ~~~~~ ~~~~~ ~~~~~ \\
		// ~~~~~ GUI functions ~~~~~ \\
		public string[] UpdateDevicesBox(bool initFirst = false)
		{
			// Abort if box is not set
			if (DevicesBox == null)
			{
				return GetDeviceNames();
			}

			DevicesBox.Items.Clear();

			DevicesBox.Items.AddRange(GetDeviceNames());
			DevicesBox.Items.Add("No device (HOST)");

			if (initFirst)
			{
				DevicesBox.SelectedIndex = 0;
			}
			else
			{
				DevicesBox.SelectedIndex = DeviceId;
			}

			// Update GUI
			UpdateVramInfo();

			return GetDeviceNames();
		}

		public long[] UpdateVramInfo()
		{
			// Abort if label or bar is not set
			if (VramInfoLabel == null || VramInfoBar == null)
			{
				return [0, 0, 0];
			}

			VramInfoLabel.Text = GetMemoryUsed(true) + " / " + GetMemoryTotal(true) + " MB";

			VramInfoBar.Maximum = (int) GetMemoryTotal(true);
			VramInfoBar.Value = (int) GetMemoryUsed(true);

			return [GetMemoryTotal(), GetMemoryFree(), GetMemoryUsed()];
		}

		public Dictionary<CUdeviceptr, Tuple<long, char>> UpdatePointersList()
		{
			// Abort if list is not set
			if (PointersList == null)
			{
				return [];
			}

			Dictionary<CUdeviceptr, Tuple<long, char>> pointers = [];
			PointersList.Items.Clear();

			foreach (KeyValuePair<CUdeviceptr, Tuple<long, char, int, int, int, string>> pointer in Pointers)
			{
				PointersList.Items.Add("<" + pointer.Value.Item2 + "> " + pointer.Key + " (" + pointer.Value.Item1 + " f32)");
				pointers.Add(pointer.Key, new Tuple<long, char>(pointer.Value.Item1, pointer.Value.Item2));
			}

			// Update label pointer count
			if (PointersInfoLabel != null)
			{
				PointersInfoLabel.Text = "Pointers: (" + pointers.Count + ")";
			}

			// Update GUI
			UpdateVramInfo();

			return pointers;
		}



		// ~~~~~ Pre-init info ~~~~~ \\
		public int GetDeviceCount()
		{
			int count = 0;

			try
			{
				count = CudaContext.GetDeviceCount();
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			return count;
		}

		public string[] GetDeviceNames()
		{
			string[] names = new string[GetDeviceCount()];

			try
			{
				for (int i = 0; i < names.Length; i++)
				{
					names[i] = CudaContext.GetDeviceName(i);
				}
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			return names;
		}


		// ~~~~~ Init ~~~~~ \\
		public void InitDevice(int id = 0)
		{
			// Dispose previous context
			Ctx?.Dispose();

			DeviceId = id;

			// Abort if id is invalid
			if (id < 0 || id >= GetDeviceCount())
			{
				return;
			}

			// Try init new context
			try
			{
				Ctx = new PrimaryContext(id);
				Ctx.SetCurrent();
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Update GUI
			UpdateVramInfo();
		}


		// ~~~~~ Context info ~~~~~ \\
		public long GetMemoryTotal(bool readable = false)
		{
			// Long memory
			long total = 0;

			// Try to get total memory
			try
			{
				total = Ctx?.GetDeviceInfo().TotalGlobalMemory ?? 0;
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Make readable
			if (readable)
			{
				return total / 1024 / 1024;
			}

			return total;
		}

		public long GetMemoryFree(bool readable = false)
		{
			// Long memory
			long free = 0;

			// Try to get free memory
			try
			{
				free = Ctx?.GetFreeDeviceMemorySize() ?? 0;
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Make readable
			if (readable)
			{
				return free / 1024 / 1024;
			}

			return free;
		}

		public long GetMemoryUsed(bool readable = false)
		{
			// Long memory
			long used = 0;

			// Try to get used memory
			try
			{
				used = GetMemoryTotal() - GetMemoryFree();
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Make readable
			if (readable)
			{
				return used / 1024 / 1024;
			}

			return used;
		}


		// ~~~~~ Pointers ~~~~~ \\
		public CUdeviceptr? AllocMemory(long length, char type = 'd', int samplerate = 44100, int bitdepth = 32, int channels = 2, string name = "")
		{
			// Abort if size is invalid
			if (length <= 0 || Ctx == null)
			{
				return null;
			}

			// Make pointer
			CUdeviceptr? pointer = null;

			// Try to allocate memory
			try
			{
				pointer = Ctx.AllocateMemory(length * sizeof(float));

				if (pointer != null)
				{
					// Add prefix to name
					if (name == "")
					{
						name = "Pointer (" + pointer.Value + ")";
					}
					else
					{
						name = name + " (" + pointer.Value + ")";
					}
					if (type == 'd')
					{
						name = "<" + type + "> " + name;
					}
					else if (type == 'c')
					{
						name = "<" + type + "> " + name;
					}
					else if (type == 'n')
					{
						name = "<" + type + "> " + name;
					}

					Pointers.Add(pointer.Value, new Tuple<long, char, int, int, int, string>(length, type, samplerate, bitdepth, channels, name));
				}
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Update GUI
			UpdatePointersList();
			return pointer;
		}

		public CUdeviceptr? PushToCuda(AudioObject track)
		{
			// Abort if track is invalid
			if (track.Data.Length == 0 || Ctx == null)
			{
				return null;
			}

			// Allocate memory
			CUdeviceptr? pointer = AllocMemory(track.Data.LongLength, 'd', track.Samplerate, track.Bitdepth, track.Channels, track.Name);

			// Abort if pointer is invalid
			if (pointer == null)
			{
				return null;
			}

			// Copy data
			try
			{
				Ctx.CopyToDevice(pointer.Value, track.Data);
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Update GUI
			UpdatePointersList();

			return pointer;
		}

		public CUdeviceptr? PushToCuda(float2[] data, int samplerate = 44100, int bitdepth = 32, int channels = 2, string name = "")
		{
			// Abort if data is invalid
			if (data.Length == 0 || Ctx == null)
			{
				return null;
			}

			// Allocate memory
			CUdeviceptr? pointer = AllocMemory(data.Length, 'c', samplerate, bitdepth, channels, name);

			// Abort if pointer is invalid
			if (pointer == null)
			{
				return null;
			}

			// Copy data
			try
			{
				Ctx.CopyToDevice<float2>(pointer.Value, data);

				// Add to pointers
				Pointers.Add(pointer.Value, new Tuple<long, char, int, int, int, string>(data.Length, 'c', samplerate, bitdepth, channels, name));
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Syncronize Ctx
			Ctx.Synchronize();

			// Update GUI
			UpdatePointersList();
			return pointer.Value;
		}

		public float2[] PushFromCuda(long pointer = 0, CUdeviceptr? ptr = null)
		{
			// Abort if pointer is invalid
			if (pointer <= 0 && ptr == null || Ctx == null)
			{
				return [];
			}

			// Get pointer
			ptr ??= new CUdeviceptr(pointer);

			// Find size and type
			if (!Pointers.ContainsKey(ptr.Value))
			{
				return [];
			}
			long length = Pointers[ptr.Value].Item1;
			char type = Pointers[ptr.Value].Item2;

			// New float2 array
			float2[] data = new float2[length];

			// Try to copy data
			try
			{
				Ctx.CopyToHost<float2>(data, ptr.Value);
				Pointers.Remove(ptr.Value);
				Ctx.FreeMemory(ptr.Value);
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Syncronize Ctx
			Ctx.Synchronize();

			// Update GUI
			UpdatePointersList();
			return data;
		}

		public float[] PullFromCuda(long pointer = 0, CUdeviceptr? ptr = null)
		{
			// Abort if pointer is invalid
			if (pointer <= 0 && ptr == null || Ctx == null)
			{
				return [];
			}

			// Get pointer
			ptr ??= new CUdeviceptr(pointer);

			// Find size and type
			if (!Pointers.ContainsKey(ptr.Value))
			{
				return [];
			}

			long length = Pointers[ptr.Value].Item1;
			char type = Pointers[ptr.Value].Item2;
			float[] data = new float[length];

			// Try to copy data
			try
			{
				Ctx.CopyToHost<float>(data, ptr.Value);
				Pointers.Remove(ptr.Value);
				Ctx.FreeMemory(ptr.Value);
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Update GUI
			UpdatePointersList();

			return data;
		}

		public long ClearPointer(long pointer = 0, CUdeviceptr? ptr = null, bool readable = false)
		{
			// Abort if pointer is invalid
			if (pointer <= 0 && ptr == null || Ctx == null)
			{
				return 0;
			}

			// Get pointer
			ptr ??= new CUdeviceptr(pointer);

			// Find size and type
			if (!Pointers.ContainsKey(ptr.Value))
			{
				return 0;
			}
			long size = Pointers[ptr.Value].Item1 * sizeof(float);
			char type = Pointers[ptr.Value].Item2;

			// Try to free memory
			try
			{
				Pointers.Remove(ptr.Value);
				Ctx.FreeMemory(ptr.Value);
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Readable size
			if (readable)
			{
				return size / 1024 / 1024;
			}

			// GC
			GC.Collect();

			return size;
		}

		internal void ClearPointers(long pointer, long spectrum)
		{
			// Abort if Ctx is not set
			if (Ctx == null)
			{
				return;
			}

			// Clear pointer if not 0
			if (pointer > 0)
			{
				ClearPointer(pointer);
			}

			// Clear spectrum if not 0
			if (spectrum > 0)
			{
				ClearPointer(spectrum);
			}

			// Update GUI
			UpdatePointersList();
		}

		public int GetPointerIndex(long pointer)
		{
			// Find index
			int index = 0;
			foreach (var ptr in Pointers)
			{
				if (ptr.Key.Pointer == pointer)
				{
					return index;
				}
				index++;
			}

			// Return -1 if not found
			return -1;
		}


		// ~~~~~ FFT ~~~~~ \\
		public CUdeviceptr? PerformForwardFFT(long pointer = 0, CUdeviceptr? ptr = null)
		{
			// Abort if pointer is invalid
			if (pointer <= 0 && ptr == null)
			{
				return null;
			}
			// Get pointer
			ptr ??= new CUdeviceptr(pointer);

			// Find size and type
			if (!Pointers.ContainsKey(ptr.Value))
			{
				return null;
			}

			// Get attributes
			long length = Pointers[ptr.Value].Item1;
			char type = Pointers[ptr.Value].Item2;
			int samplerate = Pointers[ptr.Value].Item3;
			int bitdepth = Pointers[ptr.Value].Item4;
			int channels = Pointers[ptr.Value].Item5;
			string name = Pointers[ptr.Value].Item6;

			// New pointer for FFT
			CUdeviceptr? newPtr = AllocMemory(length / 2 + 1, 'c', samplerate, bitdepth, channels, name);

			// Abort if type is not 'd'
			if (type != 'd' || newPtr == null)
			{
				return null;
			}

			// Perform FFT
			try
			{
				// Create plan
				CudaFFTPlan1D plan = new((int) length, cufftType.R2C, 1);

				// Execute plan
				plan.Exec(ptr.Value, newPtr.Value);

				// Destroy plan
				plan.Dispose();

				// Return new pointer
				return newPtr;
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Syncronize Ctx
			Ctx?.Synchronize();

			// Update GUI
			UpdatePointersList();
			return ptr;
		}

		public CUdeviceptr? PerformInverseFFT(long pointer = 0, CUdeviceptr? ptr = null)
		{
			// Abort if pointer is invalid
			if (pointer <= 0 && ptr == null)
			{
				return null;
			}

			// Get pointer
			ptr ??= new CUdeviceptr(pointer);

			// Find size and type
			if (!Pointers.ContainsKey(ptr.Value))
			{
				return null;
			}

			// New pointer for FFT
			long length = Pointers[ptr.Value].Item1;
			char type = Pointers[ptr.Value].Item2;
			CUdeviceptr? newPtr = AllocMemory(length, 'n');

			// Abort if type is not 'c'
			if (type != 'c' || newPtr == null)
			{
				return null;
			}

			// Perform FFT
			try
			{
				// Create plan
				CudaFFTPlan1D plan = new((int) length, cufftType.C2R, 1);

				// Execute plan
				plan.Exec(ptr.Value, newPtr.Value);

				// Destroy plan
				plan.Dispose();

				// Return new pointer
				return newPtr;
			}
			catch (CudaException ex)
			{
				Console.WriteLine(" --- CUDA Error: " + ex.Message);
			}

			// Syncronize Ctx
			Ctx?.Synchronize();

			// Update GUI
			UpdatePointersList();
			return ptr;
		}

		public CUdeviceptr? StretchPointerData(long pointer = 0, CUdeviceptr? ptr = null, float factor = 1.0f)
		{
			// Abort if pointer is invalid
			if (pointer <= 0 && ptr == null)
			{
				return null;
			}

			// Return if factor is 1.0
			if (factor == 1.0f)
			{
				return ptr;
			}

			// Get pointer
			ptr ??= new CUdeviceptr(pointer);

			// Find size and type
			if (!Pointers.ContainsKey(ptr.Value))
			{
				return ptr;
			}

			// Get attributes
			long length = Pointers[ptr.Value].Item1;
			char type = Pointers[ptr.Value].Item2;
			int samplerate = Pointers[ptr.Value].Item3;
			int bitdepth = Pointers[ptr.Value].Item4;
			int channels = Pointers[ptr.Value].Item5;
			string name = Pointers[ptr.Value].Item6;

			// New float2 array
			float2[] newData = new float2[(int) (length * factor)];

			// Try to copy data
			newData = PushFromCuda(0, ptr.Value);

			// Abort if data is invalid
			if (newData.Length == 0)
			{
				return ptr;
			}

			// Stretch data
			newData = StretchFloat2Data(newData, factor);

			// Push data back to CUDA
			var newPtr = PushToCuda(newData);

			// Update GUI
			UpdatePointersList();

			return newPtr;
		}

		public float2[] StretchFloat2Data(float2[] data, float factor = 1.0f)
		{
			// Return if factor is 1.0 or invalid input
			if (factor == 1.0f || data.Length == 0 || factor < 0.1f)
			{
				return data;
			}

			int newSize = (int) (data.Length * factor);
			float2[] newData = new float2[newSize];

			for (int i = 0; i < newSize; i++)
			{
				float index = i / factor;
				int lower = (int) Math.Floor(index);
				int upper = Math.Min(lower + 1, data.Length - 1);
				float fraction = index - lower;

				// Lineare Interpolation für Real- und Imaginärteil
				newData[i].x = data[lower].x + fraction * (data[upper].x - data[lower].x);
				newData[i].y = data[lower].y + fraction * (data[upper].y - data[lower].y);
			}

			return newData;
		}

	}
}