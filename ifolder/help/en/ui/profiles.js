/*
	If runtime profiling is expected in a helpset this file will add the profiling
	value to the profileArray defined below. profileArray defines one or more
	profiles that	are active for the helpset. The assignment to profileArray is
	product specific. For example, an application could replace this javascript
	file whenever the profiling needs to change.
	
	Also included is default handling of a profile passed via a 'profile' query
	parameter.

	Values added	to profileArray will correspond to profile names defined in
	the source documentation. 
*/

function getQueryParam(key, query)
{
	var re = new RegExp("[?|&]" + key + "=(.*?)&");
	var matches = re.exec(query + "&");
	
	if (!matches || matches.length < 2)
	{
		return "";
	}
	
	return decodeURIComponent(matches[1].replace("+", " "));
}

function setQueryParam(key, value, query)
{
	var q = query + "&";
	var re = new RegExp("[?|&]" + key + "=.*?&");

	if(!re.test(q))
	{
		q += key + "=" + encodeURI(value);
	}
	else
	{
		q = q.replace(re, "&" + key + "=" + encodeURIComponent(value) + "&");
	}

	q = q.replace("/^&*/",'').replace("/&*$/",'');//trim left or right ampersands
	return q[0]=="?" ? q : q = "?" + q;
}

var profileArray = Array(); //must be defined even if empty
var profileParam = getQueryParam('profile',window.location.search);//must be defined even if empty

if(profileParam.length)
{
	profileArray.push(profileParam);
}
