---
title: "WPF Game of Life"
date: 2013-04-14T08:51:24Z
slug: wpf-game-of-life
aliases: ["/wpf-game-of-life/"]
categories:
  - "Demonstration"
tags:
  - "c#"
  - "wpf"
wp_post_id: 581
---

I was introduced to cellular automatons a good decade ago through books written by Peter Small. Back then, we were using Adobe (then Macromedia) Director as our primary development platform, where object-oriented programming was still a bit of a novelty. Small's writings explained OOP using artificial life forms such as cellular automatons, with Conway's Game of Life being an oft used concrete example. I was hooked and would spend hours running patterns on a simple implementation of the game that shipped with Windows Entertainment Pack.

But I never got around to implementing the application myself until now. The game has a fairly simple model that does not take much effort to build. This is handy when learning a new platform or language, leaves the programmer free to explore the features of the framework instead of spending cycles trying to nail down the business logic correctly using an unfamiliar language syntax.

### Background

There are plenty of resources available online that describe, implement and demonstrate this game. The basic essence of the game is a two-dimensional array of cells and an endless loop that iterates over these cells to update their state depending upon some simple rules.

1. Any live cell with fewer than two live neighbours dies, as if caused by under-population.
2. Any live cell with two or three live neighbours lives on to the next generation.
3. Any live cell with more than three live neighbours dies, as if by overcrowding.
4. Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.

This implementation of the game also consists of the following features for convenience.

1. Clicking on a cell toggles its state between being alive or dead.
2. A Randomize button initializes the board by randomly setting the state of all cells to either alive or dead.
3. A Stop button lets the user stop the cell update iterations in order to observe the arrangement of the board at any given time. This is useful for times when an interesting pattern appears on the screen that seeks further observation.
4. A Next button complements the Stop button to let the user iterate to progressive generations of the board, one step at a time.

### Implementation

This implementation is made up of the following classes.

#### MainWindow.xaml and MainWindow.xaml.cs

The application window is a XAML class called MainWindow.xaml, which defines the layout of the application. The layout is built around a DockPanel that divides the screen into two - a controller toolbar and the board view.

```xml
<DockPanel Name="layout">
    <ToolBar DockPanel.Dock="Top">
        <Button Content="Start" Name="btnStart" Height="23" Width="75" Margin="5" Click="toggleStart_Click" />
        <Button Content="Next" Name="btnNext" Height="23" Width="75" Margin="5" Click="nextIteration_Click" />
        <Button Content="Clear" Name="btnClear" Height="23" Width="75" Margin="5" Click="clear_Click" />
        <Button Content="Randomize" Name="btnRandomize" Height="23" Width="75" Margin="5" Click="randomize_Click" />
    </ToolBar>
    <ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Auto">
        <life:BoardView x:Name="view" DockPanel.Dock="Top" Margin="10" Click="view_Click"></life:BoardView>
    </ScrollViewer>
</DockPanel>
```

The MainWindow class stores a reference to an instance of BoardModel as a private field. Buttons in the toolbar are wired to trigger event handlers in the MainWindow class, which in turn trigger public methods exposed by the model and view instances.

#### BoardModel.cs

The model class is where the business logic behind the application is implemented. It contains two arrays of bytes of identical length, which store the state of the board in the current iteration and the next iteration. It also initializes a timer that is used to iterate over the board state automatically. In addition, it exposes public methods to control the state of the board - Next(), Start(), Stop(), Clear() and Randomize().

The model instance dispatches an Update event every time the board changes. The window class listens for this event and passes on the contents of the new board cells to the Update() method of the view instance.

#### BoardView.cs

The view class inherits from System.Windows.FrameworkElement in order to take advantage of its built-in layout functionality. The board is drawn using low-level Visual elements. While it is easier to implement the cell drawing by using Drawings or Shapes, it also adds a lot of overhead for unnecessary features such as styles, data binding, automatic layout and input events. DrawingVisuals provide a much more performant way of drawing large numbers of objects on the screen.

```csharp
private void DrawCells()
{
    if (null == this.values)
    {
        return;
    }

    using (DrawingContext dc = this.cells.RenderOpen())
    {
        int x = 0;
        int y = 0;
        Rect rect = new Rect(OUTLINE_WIDTH, OUTLINE_WIDTH, Constants.CELL_SIZE - OUTLINE_WIDTH, Constants.CELL_SIZE - OUTLINE_WIDTH);

        for (int i = 0; i < values.Length; i++)
        {
            x = (i % Constants.CELLS_X);
            y = (i / Constants.CELLS_X);
            rect.Location = new Point((x * Constants.CELL_SIZE) + OUTLINE_WIDTH, (y * Constants.CELL_SIZE) + OUTLINE_WIDTH);

            if (1 == values[i])
            {
                dc.DrawRectangle(Brushes.Red, null, rect);
            }
        }
    }
}
```

One must fetch a reference to a DrawingContext in order to draw content onto a DrawingVisual. This is done through the RenderOpen() method of the DrawingVisual. Since we are only drawing basic rectangles, we can use the in-built DrawRectangle() method of the DrawingContext class to draw this shape for us. Internally, this method still uses a GeometryDrawing and a subclass of the Geometry class, but abstracts away those details for convenience.

The drawing is displayed on the screen by adding the Visual instances to the visual tree of the BoardView class, which is done in its constructor.

```csharp
public BoardView()
    : base()
{
    this.AddVisualChild(this.grid);
    this.AddLogicalChild(this.grid);

    this.AddVisualChild(this.cells);
    this.AddLogicalChild(this.cells);

    this.visuals = new DrawingVisual[] { this.grid, this.cells };

    this.drawGrid();
    this.drawCells();
}
```

Additionally, the BoardView class also overrides the VisualChildrenCount, GetVisualChild and MeasureOverride members of the FrameworkElement class.

```csharp
protected override int VisualChildrenCount
{
    get
    {
        return this.visuals.Length;
    }
}

protected override Visual GetVisualChild(int index)
{
    if (index < 0 || index > this.visuals.Length)
    {
        throw new ArgumentOutOfRangeException("index");
    }

    return this.visuals[index];
}

protected override Size MeasureOverride(Size availableSize)
{
    return new Size(Constants.CELLS_X * Constants.CELL_SIZE, Constants.CELLS_Y * Constants.CELL_SIZE);
}
```

The BoardView class instance also dispatches an event when the user clicks on a cell on the board. This is in turn passed over to the model instance via the window, causing the model to toggle the state of the cell that was clicked.

#### ClickEventArgs.cs

An instance of this class is passed as a parameter along with the Click event is dispatched from the BoardView class. It exposes two parameters to identify the X and Y coordinates of the cell which was clicked upon, relative to the board.

The Visual Studio solution for this project can be [downloaded from here as a ZIP archive](conwayslife.zip).
