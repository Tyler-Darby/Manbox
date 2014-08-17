var mh_running = false;

window.onload = function()
{	
	if (window.location.hash.length > 0)
	{
		mh_running = true;
		$.ajax({
			type: "POST",
			url: "/manbox/fetch",
			data: { Hash: window.location.hash }
		})
		
		.done(function(output)
		{
			mh_running = false;
			$("#input").val(output);
			runPat();
		});
	}
}

function createHashLink(hash)
{
	var endPoint = window.location.href.indexOf('#');
	if (endPoint === -1)
	{
		endPoint = window.location.href.indexOf('?');
	}
	if (endPoint === -1)
	{
		endPoint = window.location.href.length;
	}
	
	return window.location.href.slice(0, endPoint) + '#' + (hash[0] == '#' ? hash.substring(1) : hash);
}
	
function runPat()
{
	if (mh_running)
	{
		return;
	}
	
	mh_running = true;
	document.getElementById("loading").style.display = "block";
	
	var pattern = document.getElementById("input").value;
	$.ajax({
		type: "POST",
		url: "/manbox/run",
		data: { code: pattern, nsfw: "false" }
	})
	
	.done(function(response)
	{
		var json = JSON.parse(response);
		
		var outputBox = $("#output");
		outputBox.val(json["result"]);
		
		if (json["success"] == "ok")
		{
			outputBox.css("color", "#000");
		}
		else if (json["success"] == "fail")
		{
			outputBox.css("color", "#c00");
		}
		
	    $("#hash").val(createHashLink(json["hash"]));
		$("#hashfield").css("display", "block");
		
		window.location.hash = "#" + json["hash"];
		document.getElementById("loading").style.display = "none";
		mh_running = false;
	});		
}

Mousetrap.bind("ctrl+enter", runPat);