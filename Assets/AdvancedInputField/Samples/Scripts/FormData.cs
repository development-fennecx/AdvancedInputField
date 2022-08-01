//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------


namespace AdvancedInputFieldSamples
{
	public class FormData
	{
		private const string STRING_FORMAT = "Username: {0}\nPassword: {1}\nE-mail: {2}\nTelephone: {3}" +
			"\nFirst name: {4}\nLast name: {5}\nCountry: {6}\nCity: {7}\nYearly income: {8}\nHourly wage: {9}\nComments: {10}";

		public string username;
		public string password;
		public string email;
		public string telephone;
		public string firstName;
		public string lastName;
		public string country;
		public string city;
		public int yearlyIncome;
		public double hourlyWage;
		public string comments;

		public override string ToString()
		{
			return string.Format(STRING_FORMAT, username, password, email, telephone, firstName, lastName, country, city,
				yearlyIncome, hourlyWage, comments);
		}
	}
}
