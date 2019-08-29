var UPS_DATA = {}

$ ( document ).ready( function() {

	pullAllDataFromWebserver();
	
	});

function drawUpsTable()
{

	var columnOrder = ['IPAddress', 'Model','CurrentAmbientTemperature', 'AverageAmbientTemperature'];
	var columnFriendlyNames = {
	'IPAddress': 'IP Address', 
	'Hostname': 'Hostname',
	'Model': 'Model',
	'CurrentAmbientTemperature': "Room Temp.", 
	'AverageAmbientTemperature': "Avg. Room Temp."
	}


	var displayCard = $('#out');

	var tableWrapper = jQuery('<div/>', {
        type: 'div',
        class: "table-responsive",
        //html: proper(subLevels[i]),
        //onClick: 'menuHandler(\"' + level + ((level === '') ? '' : '_') + subLevels[i] + '\")'
      }).appendTo(displayCard);

	var table = jQuery('<table/>', {
		type: 'table',
		class: 'table header-border table-hover verticle-middle',
	}).appendTo(tableWrapper);

	var tHead = jQuery('<thead/>').appendTo(table);

	var tHeadRow = jQuery('<tr/>').appendTo(tHead);

	for (index in columnOrder)
	{

		jQuery('<th/>',{
			scope: 'col',
			html: columnFriendlyNames[columnOrder[index]]
		}).appendTo(tHeadRow);
	}

	var tBody = jQuery('<tbody/>').appendTo(table);

	var temp = 1;

	var allUpsKeys = Object.keys(UPS_DATA)
	for (index in allUpsKeys)
	{
		var ip = allUpsKeys[index];
		var row = jQuery('<tr/>').appendTo(tBody);
		for (col in columnOrder)
		{
			if(columnOrder[col].includes("Temperature")){
				var cell = jQuery('<td/>',{
					html: '<h5>' + UPS_DATA[ip][columnOrder[col]].toPrecision(3)  + '&deg;</h5>'
				}).appendTo(row);

				var progressBar = jQuery('<div/>',{
					class: 'progress',
					style: 'height: 20px',
				}).appendTo(cell);


				var percentage = getSafeTemperaturePercentage(UPS_DATA[ip][columnOrder[col]]);
				jQuery('<div/>',{
					class: 'progress-bar ' + getTempBarColors(percentage),
					style: 'width: ' + percentage + '%',
					html: '<b>' + UPS_DATA[ip][columnOrder[col]].toFixed(1) + '&deg;</b>',
					role: 'progressbar'
				}).appendTo(progressBar);

			}
			else
			{
				jQuery('<td/>',{
					html: UPS_DATA[ip][columnOrder[col]]
				}).appendTo(row)	

			}
		}
	}

}

function getTempBarColors(percentage)
{
	if (percentage < 30)
	{
		return "bg-primary";
	}
	else if (percentage < 75)
	{
		return "bg-success";
	}
	else if (percentage < 90)
	{
		return "bg-warning";
	}
	else
	{
		return "bg-danger";
	}
}

function getSafeTemperaturePercentage(temp)
{
	temp = parseFloat(temp);
	var maxTemp = 100.0;
	var minTemp = 32.0;

	if (temp > maxTemp)
	{
		return 100;
	}
	else if (temp < minTemp)
	{
		return 0;
	}
	else
	{
		return ((temp - minTemp)/(maxTemp - minTemp))*100;
	}

}

function pullAllDataFromWebserver()
{
	$.ajax({
		url: 'http://amonitor.example.com:3000/',
		async: true,
		method: 'POST',
		data: {
			'WebRequestType': 'DashboardDataExtract',
			'IPAddress': '0.0.0.0'
		},
		success: function(data)
		{
			parseData(data);
			drawUpsTable();
		},
		error: function (jqXHR, exception)
		{
			console.log('Ajax Request Error');
		},
			dataType: 'text'
		});
	}

function parseData(data)
{
	var dataJson = JSON.parse(data);
	for (index in dataJson)
	{
		if (dataJson[index]['IPAddress'] in UPS_DATA)
		{
			for (key in dataJson[index])
			{
				UPS_DATA[dataJson[index]['IPAddress']][key] = dataJson[index][key];
			}
		} 
		else
		{
			UPS_DATA[dataJson[index]['IPAddress']] = dataJson[index];
		}
	}

	console.log(UPS_DATA);

}