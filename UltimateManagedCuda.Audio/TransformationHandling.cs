using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaFFT;
using ManagedCuda.NVRTC;
using ManagedCuda.NvJitLink;
using ManagedCuda.VectorTypes;

namespace UltimateManagedCuda.Audio
{
	public class TransformationHandling
	{
		// ~~~~~ ~~~~~ ~~~~~ ATTRIBUTES ~~~~~ ~~~~~ ~~~~~ \\
		public string Repopath;

		public ListBox KernelList;

		public PrimaryContext Ctx;

		public List<CUmodule> Modules = [];
		public List<CudaKernel> Kernels = [];




		// ~~~~~ ~~~~~ ~~~~~ CONSTRUCTORS ~~~~~ ~~~~~ ~~~~~ \\
		public TransformationHandling(string repopath, PrimaryContext ctx, ListBox kernelList)
		{
			Repopath = repopath;
			Ctx = ctx;
			KernelList = kernelList;
		}




		// ~~~~~ ~~~~~ ~~~~~ METHODS ~~~~~ ~~~~~ ~~~~~ \\
		// ~~~~~ (I)FFT ~~~~~ \\
		public CUdeviceptr ForwardFFT(CUdeviceptr ptr, long length, bool clearInput = false)
		{
			// Abort if ptr is 0
			if (ptr.Pointer == 0)
			{
				return ptr;
			}

			// Create a new pointer for the result
			CUdeviceptr result = Ctx.AllocateMemory(length * sizeof(float));

			// Create the plan
			CudaFFTPlan1D plan = new((int) length, cufftType.R2C, 1);

			// Execute the plan
			plan.Exec(ptr, result);

			// Free the plan
			plan.Dispose();

			// Free the input if requested
			if (clearInput)
			{
				Ctx.FreeMemory(ptr);
			}

			// Return the result
			return result;
		}

		public CUdeviceptr InverseFFT(CUdeviceptr ptr, long length, bool clearInput = false)
		{
			// Abort if ptr is 0
			if (ptr.Pointer == 0)
			{
				return ptr;
			}

			// Create a new pointer for the result
			CUdeviceptr result = Ctx.AllocateMemory(length * sizeof(float));

			// Create the plan
			CudaFFTPlan1D plan = new((int) length, cufftType.C2R, 1);

			// Execute the plan
			plan.Exec(ptr, result);

			// Free the plan
			plan.Dispose();

			// Free the input if requested
			if (clearInput)
			{
				Ctx.FreeMemory(ptr);
			}

			// Return the result
			return result;
		}


		// ~~~~~ Kernel ~~~~~ \\
		public string ReadKernelString(string filepath)
		{
			// Abort if the file does not exist or isnt .txt
			if (!File.Exists(filepath) || !filepath.EndsWith(".txt"))
			{
				return string.Empty;
			}

			// Read the file
			string kernel = File.ReadAllText(filepath);

			// Return the kernel
			return kernel;
		}

		public void CompileKernel(string kernelCode, string kernelName = "kernel")
		{
			// Abort if the kernel is empty
			if (string.IsNullOrEmpty(kernelCode))
			{
				return;
			}

			// Int to store the index of the kernel & module
			int index = Kernels.Count;

			// Adust the kernel name
			if (kernelName == "kernel")
			{
				kernelName = "kernel_" + index;
			}

			// NVRTC options
			nvrtcProgram prog = new();
			NVRTCNativeMethods.nvrtcCreateProgram(ref prog, kernelCode, kernelName, 0, null, null);

			// Compile the kernel
			NVRTCNativeMethods.nvrtcCompileProgram(prog, 0, null);

			// Get the PTX size
			SizeT ptxSize = new SizeT();
			NVRTCNativeMethods.nvrtcGetPTXSize(prog, ref ptxSize);

			// Get the PTX
			byte[] ptx = new byte[ptxSize];
			NVRTCNativeMethods.nvrtcGetPTX(prog, ptx);

			// Load the module
			Modules.Add(new CUmodule());
			Ctx.LoadModulePTX(ptx, [], []);

			// Get the kernel
			Kernels.Add(new CudaKernel(kernelName, Modules[index]));

			// Destroy the program
			NVRTCNativeMethods.nvrtcDestroyProgram(ref prog);

			// Fill kernel list
			FillKernels();
		}

		public CudaKernel? GetKernelByName(string name)
		{
			// Abort if the name is empty
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			// Get the kernel
			foreach (CudaKernel kernel in Kernels)
			{
				if (kernel.KernelName.Contains(name))
				{
					return kernel;
				}
			}

			// Return null if the kernel was not found
			return null;
		}

		public void FillKernels()
		{
			// Clear the list
			KernelList.Items.Clear();

			// Fill the list
			foreach (CudaKernel kernel in Kernels)
			{
				string name = kernel.KernelName;

				if (name.Length > 20)
				{
					name = name.Substring(0, 18) + "...";
				}

				KernelList.Items.Add(name);
			}
		}

		public CUdeviceptr StretchFFT(CUdeviceptr ptr, long size, float factor, bool clearInput = false)
		{
			// Abort if ptr is 0
			if (ptr.Pointer == 0)
			{
				return ptr;
			}

			// CONST
			int BLOCK_SIZE = 256;
			int GRID_SIZE = (int) ((size * sizeof(float) * 2 + BLOCK_SIZE - 1) / BLOCK_SIZE);

			// Get new size
			long newSize = (long) (size * factor) * sizeof(float) * 2;

			// Create a new pointer for the result
			CUdeviceptr result = Ctx.AllocateMemory(newSize);

			// Get the kernel
			CudaKernel? kernel = GetKernelByName("StretchingKernel");

			// Abort if the kernel was not found
			if (kernel == null)
			{
				return ptr;
			}

			// Set the kernel parameters
			kernel.BlockDimensions = new dim3(BLOCK_SIZE, 1, 1);
			kernel.GridDimensions = new dim3(GRID_SIZE, 1, 1);

			// Execute the kernel
			kernel.Run(ptr, result, size, newSize, factor);

			// Free the input
			if (clearInput)
			{
				Ctx.FreeMemory(ptr);
			}

			// Return the result
			return result;
		}
	}
}