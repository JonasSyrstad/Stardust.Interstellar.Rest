$.UseContinuum = function (rootUrl, project, environment, serviceName, apiKey) {
    if (!$.correlationToken) $.correlationToken = "";
    if (!$.continuumUsernam) $.continuumUsernam = "";
    var apiUrl = rootUrl + "/api/v1/single/" + project + "/" + environment + "?api_key=" + apiKey;
    window.console = (function (rootUrl, project, environment, serviceName, apiKey) {
        var origConsole = window.console;
        var put = function (url, data, callback, type) {
            if (jQuery.isFunction(data)) {
                type = "PUT",
                    callback = data;
                data = undefined;
            }
            if (callback === null) callback = function () { };
            return jQuery.ajax({
                url: url,
                type: "PUT",
                method: "PUT",
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data)
            });
        };
        if (!window.console || !origConsole)
            origConsole = {};
        var isDebug = false,
        logArray = {
            logs: [],
            errors: [],
            warns: [],
            infos: []
        }
        return {
            log: function () {
                origConsole.log && origConsole.log.apply(origConsole, arguments);
                //debugger; 
                put(apiUrl,
                {
                    UserName: $.continuumUsernam,
                    CorrelationToken: $.correlationToken,
                    Message: arguments[0],
                    ServiceName: serviceName,
                    Environment: environment,
                    Properties: {}
                }).then(function (result) {

                },
                    function (error) {
                        origConsole.error(error);
                        origConsole.log(apiUrl);
                    });;
            },
            warn: function () {
                origConsole.warn && origConsole.warn.apply(origConsole, arguments);
                put(apiUrl,
                {
                    UserName: $.continuumUsernam,
                    CorrelationToken: $.correlationToken,
                    Message: arguments[0],
                    ServiceName: serviceName,
                    Environment: environment,
                    Properties: {}
                }).then(function (result) {

                },
                    function (error) {
                        origConsole.error(error);
                        origConsole
                            .log(apiUrl);
                    });;
            },
            error: function () {
                origConsole.error && origConsole.error.apply(origConsole, arguments);
                put(apiUrl,
                {
                    UserName: $.continuumUsernam,
                    CorrelationToken: $.correlationToken,
                    Message: arguments[0].split('\n')[0],
                    StackTrace: arguments[0],
                    ServiceName: serviceName,
                    Environment: environment,
                    Properties: {}
                }).then(function (result) {

                },
                    function (error) {
                        origConsole.error(error);
                        origConsole
                            .log(apiUrl);
                    });;
            },
            info: function () {

                origConsole.info && origConsole.info.apply(origConsole, arguments);
                put(apiUrl,
                {
                    UserName: $.continuumUsernam,
                    CorrelationToken: $.correlationToken,
                    Message: arguments[0],
                    ServiceName: serviceName,
                    Environment: environment,
                    Properties: {}
                }).then(function (result) {

                },
                    function (error) {
                        origConsole.error(error);
                        origConsole
                            .log(apiUrl);
                    });;
            },
            debug: function (bool) {
                isDebug = bool;
            }

        };

    }(rootUrl, project, environment, serviceName, apiKey));
    $.setUserName = function (username) {
        $.continuumUsernam = username;
    }

    $.setCorrelationToken = function (token) {
        $.correlationToken = token;
    }
}