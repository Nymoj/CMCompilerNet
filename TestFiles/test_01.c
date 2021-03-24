int calc(int a, b)
{
	return a + b;
}

int main()
{	
    int i, n, t1 : 0, t2 : 1, nextTerm;
    print("Enter the number of terms: ");
    put(n);
    print("Fibonacci Series: ");

    for i = 1 to n do 
    {
        print(t1);
        nextTerm = t1 + t2;
        t1 = t2;
        t2 = nextTerm;
    }

	return 0;
}