int foo()
{
	int b;
	return 5;
}

int main()
{
	int i : 0;
	int a : 0;

	while i < 5 do
	{
		a += foo();
		i++;
	}

	return a;
}