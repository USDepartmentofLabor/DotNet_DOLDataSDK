---
layout: page
title: ".Net DOL Data SDK"
created: 1359560059
---

<h2>Add SDK files to your project</h2>

<p><a href="https://github.com/USDepartmentofLabor/DotNet_DOLDataSDK/archive/master.zip">Extract the zipped SDK folder</a>
 or grab the <a href="http://www.dol.gov/cgi-bin/leave-dol.asp?exiturl=https://github.com/USDepartmentofLabor/DotNet_DOLDataSDK&amp;exitTitle=GitHub&amp;fedpage=no">Git repository</a>.
 Add the DOLDataUtil.cs file folder to your project.</p>

<h2>Using the SDK</h2>

<h3>Reference the DOL Data API in your source file</h3>

<p>In the source files that will make DOL Data requests add the following import:</p>

<p>using gov.dol.doldata.util;</p>

<h2>Add service references to the DOL Data API</h2>

<p>Use the Add Service Reference wizard to add a reference to the dataset(s) you want to connect to. This will generate a proxy class with strongly typed entities.</p>

<p><img alt="screen shot of add service reference menu" src="img/opa.developer.sdk.dotnet.add-reference-wizard.jpg" style="width: 325px; height: 190px; " /></p>

<h2>Making API Calls</h2>

<p>The API is accessed via a service reference proxy class.&nbsp; The DOLData API is a standard WFC Data Service (OData)
producer, but requires an authorization header to be added to every request. The class included in this SDK provides
 the authorization handling for you.</p>

<p>In order to make requests:</p>

<ol>
	<li>Define you API Key and Shared Secret in DOLDataUtil.cs</li>

{% highlight csharp %}
//Define API Key and Shared Secret in DOLDataUtil.cs 
private const string ApiKey =  "YOUR API KEY";
{% endhighlight %}

	<li>Instantiate a ServiceReference Entity object
{% highlight csharp %}
// FormsServiceReference is a proxy class created by the "Add  Service Reference"
// tool. Create new instance.
FormsServiceReference.FormsEntities fe =
new  FormsServiceReference.FormsEntities (new Uri("http://api.dol.gov/V1/FORMS"));
{% endhighlight %}
	</li>
	<li>Add an event handler to SendingRequest pointing to DOLDataUtil.service_SendingRequest
{% highlight csharp %}
//Attach event handler to the SendingRequest event
//This will take care of adding the authorization header
fe.SendingRequest  += new EventHandler<SendingRequestEventArgs>(DOLDataUtil.service_SendingRequest);
{% endhighlight %}
	</li>
	<li>Use Linq to query the data.
{% highlight csharp %}
//Use Linq to query the data
//Example: Get top 10 agencies and output to console
foreach (var form in  fe.Agency.Take(10))	
{
   Console.WriteLine(form.AgencyName);
}
{% endhighlight %}
	</li>
	<li>For service calls like SummerJobs, use proxy to execute call.
	<p>Pass the Service Operation name and paramters to the execute method.</p>

{% highlight csharp %}
string jsonData = fe.Execute<string>(new Uri(
  "getJobsListing?format=json&region=&locality=&zip=&employmentType='Any'&skipCount=1&query='Nurse'",
  UriKind.Relative)).FirstOrDefault().Replace("\\n", "").Replace("\\\"", "\"").Trim('\"');
{% endhighlight %}
	</li>
</ol>
