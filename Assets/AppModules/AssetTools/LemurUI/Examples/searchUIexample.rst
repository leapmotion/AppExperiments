
==================================
 Lemur Example: Search Results UI
==================================

------------------------------------------------
 Construct a Search Results page using Lemur UI
------------------------------------------------


Declare references for dynamic content in the layout.
-----------------------------------------------------

We'd like to define a layout to surround some search results, but we won't have the results when we first construct the layout. We're expecting those results to come in dynamically from requests to some server or local process. To fill the results in later, we can get a modifiable reference from the surrounding layout construction. ::
  Lemur.Group searchResults;

It's also possible to adjust the layout after we've built it, but we would have to navigate the layout again to find the right location. Since we're technically traversing the layout to build it now, it's quickest to get the references right away.


Chain-call to make sequences, pass arguments to make children.
--------------------------------------------------------------

We use ``Lemur.Construct()`` to begin construction of a Lemur layout. Usually layouts are organized into a 2D ``Panel()``. Each call in the context of the object returned by ``Construct()`` returns a similar object; with chain-calling, you're either modifying the last element constructed, or constructing a new element and adding it to the chain. Meanwhile, you pass another group as an argument to a group to define a parent-child relationship::

  var ui = Lemur.Construct()
    .Panel(
      Vertical(
        ScrollView(
          Vertical(
            Group(out searchResults) // empty Group definition.
          )
        )
        .Horizontal(
          TextField()  // this chain-call also results in a Group,
          .Button()    // in this case with two pre-defined elements.
        )
      )
    )
  ;

``var`` is your friend. Class names are for chumps, except when you have to declare an unitialized variable like we did earlier to use it in an out definition, but that's C#'s fault.


Define and re-use layouts within other layouts.
-----------------------------------------------

When we have a search result, let's assume we know what we generally want *in* that search result. ::

  Lemur.Hyperlink resultLink;
  Lemur.TextArea  resultText;
  var resultGroup = Lemur.Construct()
    .Panel(
      Vertical(
        Hyperlink(out resultLink)
        .TextArea(out resultText)
      )
    )
  ;

We're never going to let this particular group definition see the light of day itself. Instead, every time we get a search result from whatever other process computes and returns them, we're going to *duplicate* the definition and insert it into the results layout. ::

  var someNewResultLink = "New Result 1";
  var someNewResultDesc = "Got a new result! Hi there.";

  var newResult = resultGroup.Duplicate();
  newResult.Traverse(resultLink.Path).text = someNewResultLink;
  newResult.Traverse(resultText.Path).text = someNewResultDesc;

Some conveniences here have allowed me to traverse the duplicate, find the hyperlink and text area objects that we defined in the layout originally, and modify the content there.

We remembered the original resultLink, and theoretically we could modify that if we wanted new results to be duplicated with some default content. That reference, ``resultLink``, is associated with the original layout definition, as is ``resultText``. It has a Path defining its location in the layout - without any special modifications, this is going to be relative to the origin of the definition. Since we duplicated that group, there's a duplicate Hyperlink and a duplicate TextArea too -- instead of traversing the duplicate manually, we can just say, "Hey, go to the same place in this object that is defined as the hyperlink's place in the original object." This gets us the duplicate hyperlink, and we set its text.


Modify groups within the layout as needed.
------------------------------------------

``newResult`` has everything we want now in a single new search result; we want to add it to the search results layout. We already have the searchResults group from the very beginning, and it's already laid out. Hmm. Let's see if we can just add the new search result to the search results group::

  searchResults.Add(newResult);

Hey, perfect! That does exactly what we want it to do. Let's go! ::

  ui.Go();

``Go()`` just means "go." The UI will be constructed and displayed, but it's not like you can't modify it afterwards. Try adding another search result after the UI is active!


Appendix: Customize the layout before, during, or after construction.
---------------------------------------------------------------------

Customization is key to building the interface you're looking for. There is always a defaut set of properties for each layout element, and these defaults can be overridden within the context of a construction by passing a ``LayoutConfiguration`` to the ``Construct()`` operation. ::

  var layoutConfig = new LayoutConfiguration();
  layoutConfig.Properties<Horizontal>().padWidth = 0.1f;

  var ui = Lemur.Construct(configure: layoutConfig)
    .Panel(
      Horizontal(
        Button()
        .Button()
      )
    )
  ;

For modifying the configurations of individual elements, it's generally recommended to store special-case references and modify them directly after the layout is specified. This allows the layout definition to exist "cleanly" on its own, and configurations to be further modified later. We can do this for any given element in the layout by storing a reference to each element as we construct the definition. ::

  Panel panel;
  Horizontal horizontal;
  Button button1, button2;

  var ui = Lemur.Construct()
    .Panel(out panel,
      Horizontal(out horizontal,
        Button(out button1)
        .Button(out button2)
      )
    )
  ;

If it's more convenient, you can also name elements to be used later when the layout is constructed::

  var ui = Lemur.Construct()
    .Panel("Save/Load Menu"
      Horizontal("Horizontal Layout",
        Button("Save Button")
        .Button("Load Button")
      )
    )
  ;

Naming, as with any modifications, can also be performed after the layout definition is constructed.
