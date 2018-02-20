# Graceterm 
[![Build Status](https://travis-ci.org/mnconsulting/graceterm.svg?branch=master)](https://travis-ci.org/mnconsulting/graceterm)

Graceterm is a AspNet Core middleware wich provides implementation to ensure graceful shutdown of the application. 
It was originally written to get zero downtime while performing Kubernetes rolling updates.
The basic concept is: After aplication received a SIGTERM (a signal asking it to terminate), graceterm will hold it alive till all pending requests are completed or a timeout ocurr.

## Usage

Install Nuget package: [Graceterm](https://www.nuget.org/packages/Graceterm/)

Considering a standard AspNet Core application, edit `Configure` method of `Startup` by adding `app.UseGraceterm();` invocation. The graceterm should be on top of request pipeline to work properly, this means that you must add the `app.UseGraceterm();` invocation before any other `app.UseSomething();`.
If you are using a custom logging configuration, you should put the code wich configures log before the app.UseGraceterm() invocation in order to Graceterm generate logs according to your preferences.

```cs
using Graceterm;

public class Startup
{
    ...
    
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ...)
    {
        // custom logging configuration here

        app.UseGraceterm();

        app.UseMvc();
        
        ...
    }
    ...
}
```
### Options

You may customize Graceterm behavior providing a lambda to `app.UseGraceterm()` invocation. The available options are describled bellow:

```cs
app.UseGraceterm(options => 
{
    // How much time Graceterm will hold application alive (default is 60 seconds)
    options.TimeoutSeconds = 15; 
    
    // Requests to paths starting with the segments ("/path1", "/path2" and "/path3") 
    // will not be handled by Graceterm.
    options.IgnorePaths("/path1", "/path2", "/path3");
    
    // By default graceterm send a 503 response, with a "503 - Service unavailable" text body 
    // for requests initiated after application has asked to terminated. You may modify this
    // behavior by providing a Func<HttpContext, Task> to handle this requests.    
    options.UseCustomPosSigtermIncommingRequestsHandler(async (httpContext) =>
    {
        httpContext.Response.StatusCode = 503;
        await httpContext.Response.WriteAsync("My custom message!");
    });
});
```

You may also define a custom timeout: `app.UseGraceterm(o => o.TimeoutSeconds = 30);` (default timeout is 60 seconds).

## Contribute

Pull requests are welcome.
