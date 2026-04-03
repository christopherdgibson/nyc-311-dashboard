function scrollToTop(behavior = "smooth") {
    window.scrollTo({ top: 0, behavior: behavior });
}

function changeClassName(classNameOld, classNameNew) {
    const el = document.querySelector(`.${classNameOld}`);
    if (el) {
        el.className = classNameNew;
    }
}

function toggleClassName(element, className) {
    document.querySelector(element).classList.toggle(className);
}

//function closeNavOnClick() { // when sidebar toggled nav with class
//    document.querySelector('nav ul').addEventListener('click', function (e) {
//        if (e.target.tagName === 'A') {
//            this.classList.remove('nav-open');
//        }
//    });
//}

window.setIndeterminateSelection = (element, indeterminate) => {
    if (element) {
        element.indeterminate = indeterminate;
    }
};

function closeNavOnClick() {
    document.querySelectorAll('nav ul a').forEach(function (link) {
        link.addEventListener('click', () => {
            document.getElementById('checkNav').checked = false;
        });
    });
}

window.closeDropdownOnClickAway = (dotNetRef) => {
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.sidebar-dropdown-stack')) {
            dotNetRef.invokeMethodAsync('OnGlobalClick');
        }
    });
};

window.saveAsFile = function (fileName, bytesBase64) {
    var link = document.createElement('a');
    link.download = fileName;
    link.href = "data:application/pdf;base64," + bytesBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

window.saveElementAsPdf = function (elementId, options) {
    console.log("options:", options);
    console.log("options.image:", options.image);
    var element = document.getElementById(elementId);
    var opt = {
        margin: options.margin,
        filename: options.fileName,
        image: {
            type: options.image.type,
            quality: options.image.quality
        },
        html2canvas: {
            scale: options.html2canvas.scale
        },
        jsPDF: {
            unit: options.jsPDF.unit,
            format: options.jsPDF.format,
            orientation: options.jsPDF.orientation
        },
        pagebreak: {
            mode: options.pagebreak.mode,
            avoid: options.pagebreak.avoid
        }
    };
    html2pdf().set(opt).from(element).save();
}

window.renderApexChart = function (elementSelector, dataset, options) {
    function applyFormat(formatStr, val) {
        if (!formatStr) return val;
        if (formatStr === "integer") return val.toFixed(0);
        if (formatStr.startsWith("decimal:")) {
            const places = parseInt(formatStr.split(":")[1], 10);
            return val.toFixed(places);
        }
        // Extend here if needed
        return val;
    }

    function applyFormatter(formatters, defaultFormatter, seriesIndex, val) {
        const fallback = defaultFormatter ?? formatters?.[0];
        const fmt = formatters?.[seriesIndex] ?? fallback;
        return applyFormat(fmt, val);
    }

    var chartOptions = {
        chart: {
            type: options.chart.type,
            height: options.height,
            width: options.width
        },
        series: dataset.series,
        xaxis: {
            categories: dataset.categories
        }
    };

    if (options.xaxis) {
        chartOptions.xaxis.labels = {
            rotate: options.xaxis.labels.rotate,
            hideOverlappingLabels: options.xaxis.labels.hideOverlappingLabels,
            trim: options.xaxis.labels.trim,
            maxHeight: options.xaxis.labels.maxHeight
        };
    }

    if (options.dataLabels) {
        chartOptions.dataLabels = {
            enabled: options.dataLabels.enabled,
            formatter: function (val, { seriesIndex }) {
                return applyFormatter(options.dataLabels.seriesFormatters.formatters, options.dataLabels.seriesFormatters.fallback, seriesIndex, val);
            },
            offsetY: options.dataLabels.offsetY,
            style: {
                fontSize: options.dataLabels.style.fontSize,
                colors: options.dataLabels.style.colors
            }
        };
    }

    if (options.plotOptions) {
        chartOptions.plotOptions = {
            bar: {
                borderRadius: options.plotOptions.bar.borderRadius,
                dataLabels: { position: options.plotOptions.bar.dataLabelsPosition }
            }
        };
    }

    if (options.tooltip) {
        chartOptions.tooltip = {
            enabled: options.tooltip.enabled,
            y: {
                formatter: function (val, { seriesIndex }) {
                    return applyFormatter(options.tooltip.seriesFormatters.formatters, options.tooltip.seriesFormatters.fallback, seriesIndex, val);
                }
            }
        };
    }

    if (options.yaxis) {
        chartOptions.yaxis = {
            labels: {
                formatter: function (val) {
                    return applyFormat(options.yaxis.labelsFormatter, val);
                }
            }
        };
    }

    window.chartInstances = window.chartInstances || {};

    var chartDiv = document.querySelector(elementSelector);
    if (chartDiv) {
        // destroy existing instance if re-rendering
        if (window.chartInstances[elementSelector]) {
            window.chartInstances[elementSelector].destroy();
        }
        window.chartInstances[elementSelector] = new ApexCharts(chartDiv, chartOptions);
        window.chartInstances[elementSelector].render();
        return null;
    } else {
        return `Chart element '${elementSelector}' not found`;
    }
};

window.updateApexChart = function (options) {
    var chartDiv = document.getElementById("chart");
    console.log("updateApexChart called. Instance:", window.myApexChartInstance, "Div:", chartDiv, "Options:", options);
    if (window.myApexChartInstance && chartDiv) {
        console.log("Chart instance found ");
        //console.log("Calling updateOptions with:", options);
        //window.myApexChartInstance.updateOptions(options, true, true, true);
        console.log("Calling updateOptions with:", options.xaxis.categories);
        //window.myApexChartInstance.updateOptions({
        //    xaxis: { categories: options.xaxis.categories }
        // ...other options...
        //}, true, true, true);
        console.log("Calling updateSeries with:", options.series);
        window.myApexChartInstance.updateSeries(options.series, true);
    } else if (chartDiv) {
        console.log("Create new chart ");
        window.myApexChartInstance = new ApexCharts(chartDiv, options);
        window.myApexChartInstance.render();
    } else {
        console.error("Chart div not found!");
    }
};

window.notifyDotNetAfterChartDiv = function (dotNetRef) {
    setTimeout(() => {
        dotNetRef.invokeMethodAsync('OnChartDivReady');
    }, 0); // Or use MutationObserver?
};

window.notifyChartDivReady = function (dotNetHelper) {
    setTimeout(function () {
        dotNetHelper.invokeMethodAsync('RenderChartFromJS');
    }, 0);
}

window.disposeApexChart = function () {
    if (window.myApexChartInstance) {
        window.myApexChartInstance.destroy();
        window.myApexChartInstance = null;
    }
}