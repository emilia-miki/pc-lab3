class AtomicMethodInefficient : IMethod
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
			int chunk;

			chunk = Interlocked.Increment(ref _currentChunk) - 1;

			if (chunk >= _chunkCount)
			{
				return null;
			}

			var begin = chunk * _chunkSize;
			var end = chunk == _chunkCount - 1 ? _arrayLength : (chunk + 1) * _chunkSize;
			return (begin, end);
		}
	}

	private class Result
	{
		private long _oddIndexedSum = 0;
		private int _oddNumbersMin = int.MaxValue;
	
		public void AddToSum(int num)
		{
			Interlocked.Add(ref _oddIndexedSum, num);
		}

		public void CompareAndSetMin(int num)
		{
			var current = _oddNumbersMin;
			while (num < current)
			{
				var next = Interlocked.CompareExchange(ref _oddNumbersMin, num, current);
				if (next == current)
				{
					break;
				}

				current = next;
			}
		}

		public (long oddIndexedSum, int oddNumbersMin) GetResult()
		{
			return (_oddIndexedSum, _oddNumbersMin);
		}
	}

	private class ExecThreadObject
	{
		private ChunkManager _cm;
		private int[] _array;
		private Result _result;

		public ExecThreadObject(ChunkManager cm, int[] array, Result result)
		{
			_cm = cm;
			_array = array;
			_result = result;
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
					if (i % 2 == 1)
					{
						_result.AddToSum(_array[i]);
					}

					if (_array[i] % 2 == 1)
					{
						_result.CompareAndSetMin(_array[i]);
					}
				}
			}
		}
	}

    public (long oddIndexedSum, int oddNumbersMin) Exec(int[] array, int chunkSize)
    {
		var result = new Result();

		var cm = new ChunkManager(array.Length, chunkSize);
		
		var threads = new Thread[4];
		for (var i = 0; i < 4; i++)
		{
			var threadObject = new ExecThreadObject(cm, array, result);
			threads[i] = new Thread(threadObject.ThreadProc);
			threads[i].Start();
		}

		foreach (var thread in threads)
		{
			thread.Join();
		}

		return result.GetResult();
    }
}
