# pc-lab3

Parallel computing lab. The task is to find the sum of all odd-indexed
numbers in an array and the minimum odd number in the array. Do it using
a single-threaded method, a multithreaded method with blocking synchronization,
and a multithreaded method with non-blocking synchroniaztion using atomics.
Then measure the results and visualize them.

I've also implemented methods with an "Inefficient" suffix. Their purpose is
to require as much synchronization as possible in order to clearly show the
difference in performance when using blocking synchronization and atomics.
They are inefficient though.
