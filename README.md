<img src="https://api.travis-ci.org/Dzoukr/Shaver.svg" alt="Shaver" />

# Shaver
Shaver is lightweight library for [Suave.io](http://suave.io) web server, built on top of the [Razor Engine](https://github.com/Antaris/RazorEngine) and provides some extra features like template composing, setting custom return codes, localization resources support or server thread auto-localization by client Accept-Language header.

## Installation
First install NuGet package

    Install-Package Shaver

or using [Paket](http://fsprojects.github.io/Paket/getting-started.html)
    
    nuget Shaver

## How to use
This tutorial covers only Shaver additional features and considers you are familiar with Razor Engine syntax. 

### Templating
All functions related to templating are located in `Shaver.Razor` module.

#### Single page
To render single page, use Shaver in exactly the same way as you would use Razor in Suave:
    
    open Shaver.Razor
    
    type MyModel = { Message: string }
    
    let webPart : WebPart = singlePage "SinglePage.html" { Message = "Hello Shaver"}
    
#### Single page with custom HTTP code
In some cases you just want to return your page with other than `HTTP 200` response code (e.g. for failed authorization):

    singlePageWithCode HTTP_401 "Unauthorized.html" { Message = "Sorry, man"}
    
#### Master page
You do not have to rely just on single template. Templates can be composed together to create whole page without  having "supermodels" (models wrapping other models) in your code.

Let`s have two small templates:

##### Menu.html

    <ul>
      @foreach(var item in @Model) {
        <li>@item</li>
      }
    </ul>
    
##### Content.html

    <div>Hello @Model.Name let`s make some noise in @Model.City</div>
    
And now we create master template with placeholders for both partial templates:

##### Master.html

    <body>
      <h1>@Model.Title</h1>
      <div id="menu">{{{SectionOne}}}</div>
      <div id="content">{{{SectionTwo}}}</div>
    </body>
    
Ok, now it is time to compose everything together and create full page:
    
    let menuItems = ["About"; "Contact us"] 
    type MyContentModel = { Name: string; City: string }
    type MyMasterModel = { Title: string }
    
    let webPart : WebPart =
      [
          ("SectionOne", partial "Menu.html" menuItems);
          ("SectionTwo", partial "Content.html" { Name = "Roman"; City = "Prague"  })
      ] 
      |> masterPage "masterPage.html" { Title = "Welcome" }
      
#### Master page with custom HTTP code
To return custom HTTP code, use `masterPageWithCode` function:

     ...
     |> masterPageWithCode HTTP_401 "masterPage.html" { Title = "An error occured" }
     
#### Empty partials
Sometimes you may want to have some section of master template rendered as empty string. You do not have to create new empty template. Just use `empty` instead:

    let webPart : WebPart =
      [
          ("SectionOne", empty);
          ("SectionTwo", empty)
      ] 
      |> masterPage "masterPage.html" { Title = "Welcome" }
      
#### Nested master pages
Your master templates can be composed using other master templates by using `nested` function:

    let webPart : WebPart =
      [    
        ("SectionOne", partial "Menu.html" menuItems);
        ("SectionTwo",
          [("NestedOne", partial "partialOne.html" { One = "Hello Nested One" });
           ("NestedTwo", partial "partialTwo.html" { Two = "Hi Nested Two" })
          ] |> nested "nestedMasterPage.html" {Nested = "Hi Nested"});
      ]
      |> masterPage "masterPage.html" { Title = "Welcome" }
      
### Localization
Shaver library provides functionality to enable auto-localization by parsing `Accept-Language` request header. All related functions are located in `Shaver.Localization` module.

#### Current UI culture
To set current culture (`System.Threading.Thread.CurrentThread.CurrentUICulture`) use `localizeUICulture` function as shown in example below:

    ...
    open Shaver.Razor
    open Shaver.Localization

    let webPart =
      localizeUICulture >> // <-- Setting current UI culture
      choose [
        path "/" >=> singlePage "SinglePage.html" { Message = "Hello Shaver"}
      ]
    
    startWebServer defaultConfig webPart
    
#### Current culture
If you want to change current culture, use `localizeCulture` instead:

    ...
    localizeCulture >> // <-- Setting current culture
      choose [
        path "/" >=> singlePage "SinglePage.html" { Message = "Hello Shaver"}
      ]
    ...
    
### Resources
Models are great for sending dynamic data to templates, but usually you don`t want to pass static texts for labels, headings, etc... Having separated localized resource files is much better and you can even use them from frontend!

Let\`s create two localization json files and put them in `<Root>\Strings` (default) folder:

##### Greetings.json

    {
      "Welcome" : "Welcome, english speaking visitor!"
    }

##### Greetings.cs-CZ.json

    {
      "Welcome" : "Nazdar, tady mluvíme česky!"
    }
    
Now we can use these values directly in our template:
    
    <h1>{{{$:Greetings:Welcome}}}</h1>
    
As you may expect, these values will be replaced with values from correct localized resource json file.

**Please note:**
* Correct file selection is based on current UI culture, so don\`t forget to use `localizeUICulture` function as described in *Localization* section.
* By default, files are expected to be located in `<Root>\Strings` folder. This can be changed:

        Shaver.Resources.folder <- "MyResourcesFolder"
        
* You can have many culture specific variations. 

    * `Greetings.json` (default for all cultures)
    * `Greetings.en.json` (default for all english)
    * `Greetings.en-US.json` (US english)
    * `Greetings.en-GB.json`. (Great Britain english)
     
    Search logic works from the most specific culture to default one. So if your page visitor comes from United States (having culture code set to `en-US`), `Greetings.en-US.json` is used (exact match!). If he comes from Australia (having culture code set to `en-AU`), `Greetings.en.json` is used, because it is the default resource for english visitors. If he comes from France (having culture code set to `fr-FR`), default `Greetings.json` is used (no french resource found).

* Do not forget to set *Copy to Output Directory* on all your localization json files 
 
## Configuration
If you don\`t feel comfortable with `{{{` and `}}}` for templating, you can configure your own opening and closing tags:

    Shaver.Razor.openTag <- "[[["
    Shaver.Razor.closeTag <- "]]]"
    
Your template code would look like this:
    
    <h1>[[[$:Greetings:Welcome]]]</h1>
    
Please note that this would also affect section definition in templates:

    <body>
      <h1>@Model.Title</h1>
      <div id="menu">[[[SectionOne]]]</div>
      <div id="content">[[[SectionTwo]]]</div>
    </body>
    
## Contribution
Did you find any bug? Missing functionality? Please feel free to [Create issue](https://github.com/Dzoukr/Shaver/issues) or [Pull request](https://github.com/Dzoukr/Shaver/pulls).
