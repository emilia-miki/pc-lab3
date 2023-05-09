class AtomicMethod : IMethod
{
	private class ChunkManager
	{
		private int _currentChunk = 0;

		private int _chunkCount;
		private int _chunkSize;
		private int _arrayLength;

		public ChunkManager(int arrayLength, int chunkSize)
		{
			_arrayLength = arrayLength;
			_chunkSize = chunkSize;
			_chunkCount = _arrayLength / _chunkSize;
		}

		public (int begin, int end)? GetAndIncrement()
		{
			var chunk = Interlocked.Increment(ref _currentChunk) - 1;

			if (chunk >= _chunkCount)
			{
				return null;
			}

			var begin = chunk * _chunkSize;
			var end = chunk == _chunkCount - 1 ? _arrayLength : (chunk + 1) * _chunkSize;
			return (begin, end);
		}
	}

	private class Results<T>
	{
		private object _lockObj = new object();
		private List<T> _results = new List<T>();

		public IReadOnlyList<T> Get => _results;
		
		public void Add(T result)
		{
			lock (_lockObj)
			{
				_results.Add(result);
			}
		}
	}

	private class FindOddIndexedSumThreadObject
	{
		private ChunkManager _cm;
		private int[] _array;
		private Results<long> _results;
		private long _sum = 0;

		public FindOddIndexedSumThreadObject(ChunkManager cm, int[] array, Results<long> results)
		{
			_cm = cm;
			_array = array;
			_results = results;
		}

		public void ThreadProc()
		{
			while (true)
			{
				var bounds = _cm.GetAndIncrement();

				if (!bounds.HasValue)
				{
					break;
				}

				var begin = bounds.Value.begin;
				var end = bounds.Value.end;

				if (begin % 2 == 0)
				{
					begin += 1;
				}

				for (var i = begin; i < end; i += 2)
				{
					_sum += _array[i];
				}
			}

			_results.Add(_sum);
		}
	}


	private class FindOddNumbersMinThreadObject
	{
		private ChunkManager _cm = null!;
		private int[] _array;
		private Results<int> _results;
		private int _min = int.MaxValue;

		public FindOddNumbersMinThreadObject(ChunkManager cm, int[] array, Results<int> results)
		{
			_cm = cm;
			_array = array;
			_results = results;
		}
		
		public void ThreadProc()
		{
			while (true)
			{
				var bounds = _cm.GetAndIncrement();

				if (!bounds.HasValue)
				{
					break;
				}

				var begin = bounds.Value.begin;
				var end = bounds.Value.end;

				for (var i = begin; i < end; i++)
				{
					if (_array[i] % 2 == 1 && _array[i] < _min)
					{
						_min = _array[i];
					}
				}
			}

			_results.Add(_min);
		}
	}


    public (long oddIndexedSum, int oddNumbersMin) Exec(int[] array, int chunkSize)
    {
		var oddIndexedSums = new Results<long>();
		var oddNumbersMins = new Results<int>();

		var oddIndexedSumChunkManager = new ChunkManager(array.Length, chunkSize);
		var oddNumbersMinChunkManager = new ChunkManager(array.Length, chunkSize);
		
		var threads = new Thread[4];
		for (var i = 0; i < 2; i++)
		{
			var findOddIndexedSumThreadObject = new FindOddIndexedSumThreadObject(
				oddIndexedSumChunkManager, array, oddIndexedSums);
			threads[i * 2] = new Thread(findOddIndexedSumThreadObject.ThreadProc);
			threads[i * 2].Start();

			var findOddNumbersMinThreadObject = new FindOddNumbersMinThreadObject(
				oddNumbersMinChunkManager, array, oddNumbersMins);
			threads[i * 2 + 1] = new Thread(findOddNumbersMinThreadObject.ThreadProc);
			threads[i * 2 + 1].Start();
		}

		foreach (var thread in threads)
		{
			thread.Join();
		}

		if (oddIndexedSums.Get.Count != 2)
		{
			throw new Exception($"oddIndexedSums.Count is {oddIndexedSums.Get.Count}, but must be 2");
		}

		if (oddNumbersMins.Get.Count != 2)
		{
			throw new Exception($"oddNumbersMins.Count is {oddNumbersMins.Get.Count}, but must be 2");
		}

		long oddIndexedSum = 0;
		foreach (var sum in oddIndexedSums.Get)
		{
			oddIndexedSum += sum;
		}

		int oddNumbersMin = int.MaxValue;
		foreach (var min in oddNumbersMins.Get)
		{
			oddNumbersMin = oddNumbersMin < min ? oddNumbersMin : min;
		}

		return (oddIndexedSum, oddNumbersMin);
    }
}
