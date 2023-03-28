Goal: create static html for views in the app.   

Why: to make it easy for designers to update the mvc app without having to install a thing.  Also useful for things like translation or html/css refactoring, or validating all your possible markup.

Strategy:  Via command line.   Mocked data.  No pipeline required.  Similar to unit testing.

Ideally: Links will work, like /productions/4565 becomes /productions_details.html.  Optionally copy all assets.  

There are various approaches.  Precompiling views helps, since asp runtime need not run.  If the views are compiled, you could technically just render the views out individually.  The problem is that actions can share views.  Rendering the possible views for each action might be a better way to go.  Each action has 0-x views.  Mock the data in a way that you can render all the views.  

Problems without pipeline:
	- Routes & other urls don't resolve properly
	- Significant mocking required
	- Web.config is not read

Also, no exceptions should be thrown.  So actually you should have unit tests for the ripper, or something…

You should be able to test a view in various different contexts.  Like, nobody logged in, a guest only, an account holder (with multiple different statuses).  
It will also be helpful to test languages, html validation.  Should also be used to test API, outputting xml or json files.  

It also should be split up by use case.  For example, "Creating Account from Demo", then do two separate ones, one for subscribing and not.
Maybe generate some JS so that if you are viewing a page, ou can do ctrl+arrow to go back and forth between steps.  Also file names should be prepended with numbers to indicate order within the use case.

In the output files, there should be information about the url & context that was provided.  This could be a comment injected into output.  Could also be nice to have some JS that responds to a key combo to display the context.

Other desired features:
