using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Collections;
namespace BasicServerHTTPlistener
{
    internal class Program
    {

        private static void Main(string[] args)
        {




            //if HttpListener is not supported by the Framework
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("A more recent Windows version is required to use the HttpListener class.");
                return;
            }
 
 
            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            if (args.Length != 0)
            {
                foreach (string s in args)
                {
                    listener.Prefixes.Add(s);
                    // don't forget to authorize access to the TCP/IP addresses localhost:xxxx and localhost:yyyy 
                    // with netsh http add urlacl url=http://localhost:xxxx/ user="Tout le monde"
                    // and netsh http add urlacl url=http://localhost:yyyy/ user="Tout le monde"
                    // user="Tout le monde" is language dependent, use user=Everyone in english 

                }
            }
            else
            {
                Console.WriteLine("Syntax error: the call must contain at least one web server url as argument");
            }
            listener.Start();

            // get args 
            foreach (string s in args)
            {
                Console.WriteLine("Listening for connections on " + s);
            }

            // Trap Ctrl-C on console to exit 
            Console.CancelKeyPress += delegate {
                // call methods to close socket and exit
                listener.Stop();
                listener.Close();
                Environment.Exit(0);
            };


            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string documentContents;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                    }
                }
                
                // get url 
                Console.WriteLine($"Received request for {request.Url}");

                //get url protocol
                Console.WriteLine(request.Url.Scheme);
                //get user in url
                Console.WriteLine("user : " + request.Url.UserInfo);
                //get host in url
                Console.WriteLine("host :" + request.Url.Host);
                //get port in url
                Console.WriteLine(request.Url.Port);
                //get path in url 
                Console.WriteLine("localpath :" + request.Url.LocalPath);


                // parse path in url 
                Console.WriteLine("Segments :");
                foreach (string str in request.Url.Segments)
                {
                    Console.WriteLine(str);
                }

                //get params un url. After ? and between &
                
                MyReflectionClass c = new MyReflectionClass();
                
                if (request.Url.Segments.Length==3 && c.GetType().GetMethod((string)request.Url.Segments[2]) !=null)
                {
                    Console.WriteLine("param1 = " + HttpUtility.ParseQueryString(request.Url.Query).Get("param1"));
                    Console.WriteLine("param2 = " + HttpUtility.ParseQueryString(request.Url.Query).Get("param2"));

                    ArrayList parameters = new ArrayList();
                    parameters.Add(HttpUtility.ParseQueryString(request.Url.Query).Get("param1"));
                    parameters.Add(HttpUtility.ParseQueryString(request.Url.Query).Get("param2"));

                    string methodName = request.Url.Segments[2];

                    Console.WriteLine("methode nom : " + methodName);

                    // Construct a response.
                    Type type = typeof(MyReflectionClass);
                    MethodInfo method = type.GetMethod(methodName);
                    string result = "";
                    if (methodName.Equals("MyMethod"))
                    {
                        result = (string)method.Invoke(c, parameters.ToArray());
                    }
                    else if (methodName.Equals("MyMethod2"))
                    {
                        result = (string)method.Invoke(c, parameters.ToArray());
                    }
                    else if (methodName.Equals("callExternalExe"))
                    {
                        result = (string)method.Invoke(c, new string[] { (string) parameters[0] });
                    }

                    Console.WriteLine(documentContents);

                    // Obtain a response object.
                    HttpListenerResponse response = context.Response;

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(result);
                    // Get a response stream and write the response to it.
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    // You must close the output stream.
                    output.Close();
                    
                    


                }
            }
            // Httplistener neither stop ... But Ctrl-C do that ...
            listener.Stop();
        }
    }

    public class MyReflectionClass
    {
        // http://localhost:8080/foo/MyMethod?param1=oui&param2=non
        public string MyMethod(string param1, string param2)
        {
            Console.WriteLine("Call MyMethod");
            return "<HTML><BODY>Hello " + param1 + " " +  param2 + "</HTML></BODY>";
        }
        // http://localhost:8080/foo/MyMethod2?param1=oui&param2=non
        public string MyMethod2(string param1, string param2)
        {
            Console.WriteLine("Call MyMethod");
            return "<HTML><BODY>Hello2222 " + param1 + " " + param2 + "</HTML></BODY>";
        }
        // http://localhost:8080/foo/callExternalExe?param1=oui
        public string callExternalExe(string param1)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"D:\4A\S8\SOC\RepoTD\TD2\ExecTest\bin\Debug\ExecTest.exe"; // Specify exe name.
            start.Arguments = param1; // Specify arguments.
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            //
            // Start the process.
            //
            using (Process process = Process.Start(start))
            {
                //
                // Read in all the text from the process with the StreamReader.
                //
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                    return "<HTML><BODY>L'éxécutable s'est lancé avec comme param : " + result + "</HTML></BODY>";
                }
            }
            

        }
    }
}