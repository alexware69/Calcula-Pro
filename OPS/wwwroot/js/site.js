// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function() {
	$('.myElements').wookmark({
			align: 'left',
			autoResize: true,
			comparator: null,
			container: $('#tiles'),
			direction: undefined,
			ignoreInactiveItems: true,
			itemWidth: "15%",
			fillEmptySpace: true,
			flexibleWidth: 0,
			offset: 90,
			onLayoutChanged: undefined,
			outerOffset: 20,
			possibleFilters: [],
			resizeDelay: 50,
			verticalOffset: 20
	});
});