int foo()
{
	int b;
	return 0;
}

int main()
{
	int i : 0;
	int a : 0;

	while i < 200000 do
	{
		a += 2;
		i++;
	}

	return a;
}