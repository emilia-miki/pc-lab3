class SingleThreadMethod : IMethod
{
	public (long oddIndexedSum, int oddNumbersMin) Exec(int[] array, int chunkSize)
	{
		long oddIndexedSum = 0;
		for (var i = 1; i < array.Length; i += 2)
		{
			oddIndexedSum += array[i];
		}

		var oddNumbersMin = int.MaxValue;
		foreach (var number in array)
		{
			if (number % 2 == 1 && number < oddNumbersMin)
			{
				oddNumbersMin = number;
			}
		}

		return (oddIndexedSum, oddNumbersMin);
	}
}
