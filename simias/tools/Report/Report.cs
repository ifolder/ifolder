using System;
using System.IO;
using System.Xml;

/// <summary>
/// Parses test results and creates a report.
/// </summary>
public class Report 
{
	string dir;
	TextWriter writer;
	int totalCases;
	int totalRuns;
	int totalPasses;
	double totalTime;
	string failsList;

	public Report(string dir, TextWriter writer)
	{
		this.dir = dir;
		this.writer = writer;
		this.failsList = "";
	}

	int Parse()
	{
		if (Directory.Exists(dir))
		{
			String[] files = Directory.GetFiles(dir, "*.Test.xml");

			foreach(string file in files)
			{
				writer.WriteLine("Parsing: {0}", 
					Path.GetFileName(file));

				ParseTestOutputFile(file);
			}
		}
		return 0;
	}

	int ParseTestOutputFile(string file)
	{
		int cases = 0;
		int runs = 0;
		int passes = 0;
		int fails = 0;

		// find a component name
		string name = Path.GetFileNameWithoutExtension(file);
		name = Path.GetFileNameWithoutExtension(name);

		// parse the XML
		XmlDocument doc = new XmlDocument();
		doc.Load(file);
		
		foreach (XmlElement node in doc.GetElementsByTagName("test-case"))
		{
			cases++;
	
			if ("True".Equals(node.GetAttribute("executed")))
			{
				runs++;
			}

			if ("True".Equals(node.GetAttribute("success")))
			{
				passes++;
			}

			// not all test-case elementes have a time
			string time = node.GetAttribute("time"); 
			if ((time != null) && (time.Length > 0))
			{
				totalTime += Double.Parse(time);
			}
		}
		
		// fails
		if ((fails = runs - passes) > 0)
		{
			// append to the fails list
			failsList = String.Format("{0}\n{1,4}: {2}", failsList, fails, name);
		}

		// sum up
		totalCases += cases;
		totalRuns += runs;
		totalPasses += passes;

		return 0;
	}

	int WriteSummary()
	{
		const string header = "----- TEST REPORT -----";
		// summary
		writer.WriteLine();
		writer.WriteLine(header);
		writer.WriteLine();
		writer.WriteLine("TEST CASE SUMMARY");
		writer.WriteLine();
		writer.WriteLine("  Total: {0}", totalCases);
		writer.WriteLine("    Ran: {0}", totalRuns);
		writer.WriteLine(" Passed: {0} [ {1:p1} ]", totalPasses,
			(double)totalPasses/(double)totalRuns);
		writer.WriteLine();
		writer.WriteLine("   Time: {0} seconds", totalTime);
		
		// specifics
		writer.WriteLine();
		writer.WriteLine("FAILED TEST CASES");
		writer.WriteLine(failsList);
		writer.WriteLine();
		writer.WriteLine(header);
		writer.WriteLine();

		return 0;
	}

	[STAThread]
	static int Main(string[] args)
	{
		int result = 0;

		// check arguments
		if (args.Length != 1)
		{
			Console.WriteLine("USAGE: report directory");
			return 1;
		}

		Report report = new Report(args[0], Console.Out);
		report.Parse();
		report.WriteSummary();

		return result;
	}
}


