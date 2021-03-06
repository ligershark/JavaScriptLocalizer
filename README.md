JavaScriptLocalizer
===================

Localizes strings inside JavaScript files when they run though ASP.NET Optimization.

It works with any `ResourceManager` types, including Resource files (.resx).

The JavaScriptLocalizer will determine what language to apply by examing variuos things about the current request.

The order of which it determins the language is:

* The `QueryString` parameter "lang"
* The `Aceept-Language` HTTP header
* and finally the application defaults

## Setup ##

The JavaScriptLocalizer plugs in to the BundleTable. To use it you must first declare your bundles: 

    ScriptBundle scripts = new ScriptBundle("~/locscript");
    scripts.Include("~/scripts/loc1.js", "~/scripts/loc2.js");

Then add the `LocalizerTransform` to the bundle and specify your resource:

    scripts.Transforms.Add(new LocalizationTransform(typeof(Resources.text)));

In this case, a .resx file with the name `text` is used.

And finally add the `ScriptBundle` to the `BundleTable`:

    BundleTable.Bundles.Add(scripts);

Now you have the bundles configured to use the JavaScriptLocalizer. 

You need to use the `LocScript.Render()` method to include your bundled scripts on your pages:

    @LocScripts.Render("~/locscript")

That will automatically append the current language as a URL parameter and produce this output:

    <script src="/locscript?lang=en-US&v=Fn60oK0d4Rp7vKzYVdh4fKui3uBPccGcZXSmges4aac1"></script>

## Using it in JavaScript ##
The JavaScriptLocalizer looks for patterns in your script files to localize. The pattern uses colons and looks like this:

    demo.innerHTML = "::nameOfPage::";

The above maps directly to the resx key	`Resources.text.nameOfPage`.