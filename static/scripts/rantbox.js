var mh_running = false;

window.onload = function()
{	
	var cbNsfw = document.getElementById("nsfw");
	cbNsfw.checked = $.cookie("nsfw") === "true";
	cbNsfw.onchange = function() { $.cookie("nsfw", cbNsfw.checked); }

	if (window.location.hash.length > 0)
	{
		mh_running = true;
		$.ajax({
			type: "POST",
			url: "/rantbox/fetch",
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
	var loading = document.getElementById("loading");
	loading.style.visibility = "visible";
	loading.style.opacity = 100;
	
	var pattern = document.getElementById("input").value;
	var dirty = document.getElementById("nsfw").checked;
	
	$.ajax({
		type: "POST",
		url: "/rantbox/run",
		data: { code: pattern, nsfw: dirty }
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
		
		var loading = document.getElementById("loading");
		loading.style.visibility = "hidden";
		loading.style.opacity = 0;
		
		window.location.hash = "#" + json["hash"];		
		mh_running = false;
	});		
}

Mousetrap.bind("ctrl+enter", runPat);