// sum all odd elements of an array and find the smallest odd number
// write it synchronously and time it
// write it using blocking synchronization primitives and time it
// write it using atomics and CAS operations and time it
// repeat the previous 3 steps with different fata sizes and plot the results
// derive conclusions from the results

using System.Diagnostics;
using System.Text;

const int randMin = 0;
const int randMax = 10_000;
IEnumerable<IMethod> methods = AppDomain.CurrentDomain.GetAssemblies()
  .SelectMany(x => x.GetTypes())
  .Where(x => typeof(IMethod).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
  .Select(x => (IMethod) Activator.CreateInstance(x)!);

var results = new StringBuilder();
results.AppendLine("size,chunkSize,method,ticks,error?");
var rand = new Random(42);
var sizes = new int[] { 1000, 10_000, 100_000, 1_000_000, 10_000_000, 100_000_000 };
var chunkSizes = new int[] { 10, 100, 500, 1000, 10000, 100000, 1_000_000 };
foreach (var size in sizes)
{
    Debug.WriteLine($"Initializing an array of size {size}");
    var array = new int[size].Select(_ => 
        rand.Next(randMin, randMax)).ToArray();
    var reference = (array.Where((_, i) => i % 2 == 1).Select(num => (long) num).Sum(), array.Where(num => num % 2 == 1).Min());
    foreach (var chunkSize in chunkSizes)
    {
        if (size < chunkSize * 2)
        {
            break;
        }

        Debug.WriteLine($"Testing with chunk size {chunkSize}");

        foreach (var method in methods)
        {
            var name = method.GetType().Name;
            Debug.WriteLine($"Testing {name}");

            (long, int) calcResult = (0, 0);
            long ticks = 0;
            var error = string.Empty;

            try
            {
                (calcResult, ticks) = Time(method, array, chunkSize);
            }
            catch (Exception e)
            {
                error = $"Error: Method {name}: {e.Message}";
            }

            if (error == string.Empty && calcResult != reference)
            {
                error = $"Error: Method {name} returned the wrong result! " +
                    $"Expected: {reference}, Actual: {calcResult}";
            }

            results.AppendLine($"{size},{chunkSize},{name},{ticks},{error}");
        }
    }
}
Debug.WriteLine("");

Debug.WriteLine("Testing complete. Results are as follows:");

results.Remove(results.Length - 1, 1);
var resultsString = results.ToString();

Debug.WriteLine(resultsString);

using (var csv = File.Open("results.csv", FileMode.Create))
{
    using (var writer = new StreamWriter(csv))
    {
        writer.Write(resultsString);
    }
}

((long sumOfOddIndexed, int minOddNumber), long ticks) Time(IMethod method, int[] array, int chunkSize)
{
    Debug.WriteLine($"Testing {method.GetType().Name} on array of size {array.Length} with chunk size {chunkSize}");
    var sw = new Stopwatch();
    sw.Start();
    var result = method.Exec(array, chunkSize);
    sw.Stop();
    return (result, sw.ElapsedTicks);
}