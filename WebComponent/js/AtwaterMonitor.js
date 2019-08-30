var UPS_DATA = {};
var LINE_CHART = null;
$ ( document ).ready( function() {

	pullAllDataFromWebserver();


	$('#TempHistoryButton').on('click', function() {

		presentDrillDownModal('10.10.180.103')


	})
	});

function presentDrillDownModal(ipAddress){
	$('#drill-down-modal').modal('show');
	$('[name=modal-ip-address]').html(ipAddress);
	$.ajax({
		url: 'http://amonitor.example.com:3000/',
		async: true,
		method: 'POST',
		data: {
			'WebRequestType': 'GetTemperatureHistory',
			'IPAddress': ipAddress
		},
		success: function(data)
		{
			let temperatureData = JSON.parse(data);
			drawChart('ambientTemperatureDisplayChart',temperatureData);

		},
		error: function (jqXHR, exception)
		{
			console.log('Ajax Request Error');
		},
		dataType: 'text'
	});
}

function drawChart(id,data) {

	let labels = [];
	let dataToDraw = [];

	for(let e in data){
		labels.push(data[e]["TimeStamp"].match(/T([\d:]+)/g)[0].substring(1));
		dataToDraw.push(data[e]["Temperature"]);
	}

	//line chart
	let ctx = document.getElementById(id);
	//ctx.height = $( document ).width()/20;
	let ctxContext = document.getElementById(id).getContext("2d");

	if(LINE_CHART !== null){
		LINE_CHART.destroy();
	}

	let gradientStroke = ctxContext.createLinearGradient(5,5,25,400);

	gradientStroke.addColorStop(0, "#E71D36");


	gradientStroke.addColorStop(0.35, "#F29D56");
	gradientStroke.addColorStop(0.7, "#71F79F");
	gradientStroke.addColorStop(1, "#0FA3B1");

	//gradientStroke.addColorStop(1, "#011627");


	LINE_CHART = new Chart(ctx, {
		type: 'line',
		data: {
			//labels: ["January", "February", "March", "April", "May", "June", "July"],
			labels: labels,
			datasets: [
				{
					label: "My First dataset",
					borderColor: "rgba(0,	0, 0,.4)",
					borderWidth: "3",
					backgroundColor: gradientStroke,//"rgba(242,	157, 86,.7)",
					data: dataToDraw
				}
			],
		},
		options: {
			responsive: true,
			tooltips: {
				mode: 'index',
				intersect: false,
				callbacks: {
					label: (item) => `${item.yLabel}°F`
				}
			},
			hover: {
				mode: 'nearest',
				intersect: true
			},
			legend: {
				display: false
			},
			scales: {
				yAxes: [{
					ticks: {
						min: 30,
						max: 100,
					}
				}]
			}

		}
	});

}

function drawUpsTable()
{

	var columnOrder = ['Hostname','CurrentAmbientTemperature','IPAddress','Model'];
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
		class: 'table header-border table-striped table-hover verticle-middle',
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
		if (UPS_DATA[ip]['Model'] === 'Unknown') {
			continue;

		}
		var row = jQuery('<tr/>',{
			class: 'clickable-entry',
			id: 'dev-row-'+ ip
		}).appendTo(tBody);
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
	$('.clickable-entry').on('click', function () {
		presentDrillDownModal(this.id.replace('dev-row-',''));
	})

}

function getTempBarColors(percentage)
{
	if (percentage < 30)
	{
		return "bg-primary";
	}
	else if (percentage < 70)
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
}