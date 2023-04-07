function Print_Create()
{
	return new scriptObject(){class = "Print";};
}

function Print::Clear(%print)
{
	for(%i = 0; %i < 25; %i++)
	{
		%print.s[%i] = "";
	}
	return %print;
}

function Print::Set(%print,%n,%string)
{
	%print.s[%n] = %string;
	return %print;
}

function Print::Get(%print)
{
	%string = "";
	for(%i = 0; %i < 25; %i++)
	{
		%string = %string @ %print.s[%i];
	}

	return %string;
}
