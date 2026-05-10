---
title: "Adding Some Life"
date: 2016-12-22T01:11:00Z
slug: adding-some-life
aliases: ["/adding-some-life/"]
categories:
  - "Demonstration"
tags:
  - "c#"
  - "fitnet"
  - "wpf"
wp_post_id: 1006
---

The previous installment in the Building FitNet series explained how to connect a view-model with a view, using a modular and extensible architecture. In this episode, I explain how interactivity can be added to the user interface.

The simplest interactive UI control is a button. You click on it, and it executes some action. Buttons in Windows Forms worked the same way. But WPF takes the button and turns the knob up to 11. Buttons in WPF can take on literally any appearance that you can dream of. Building the user interface using XAML is already so much better than using the Designer or literal code statements. Features such as data binding are added bonus and allow for more dynamism in the user interface. You can bind the appearance of a Button instance to a view-model that reacts to the state of the application and literally make things dance.

The change in architecture also extends to the use of the event handlers. The built-in System.Windows.Controls.Button class has a property called Command, which can be bound to the view-model of the instance. The Command property is of type System.Windows.Input.ICommand. Any class that implements this interface can be attached to this property.

This sounds a bit counterintuitive at first. Why have a special type, when a simple function can suffice?

And the answer is separation of concerns. A command decouples the object that invokes the command from the object that executes the command.

The ICommand interface also provides additional properties that make it easy to control the state of the button with a bit of additional code.

```csharp
public class RelayCommand : ICommand
{
    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        ...
        _execute = execute;
        _canExecute = canExecute;
    }
 
    event EventHandler ICommand.CanExecuteChanged
    {
        // Event dispatched event when the state of the CanExecute property changes
        ...
    }
 
    bool ICommand.CanExecute(object parameter)
     {
         return null == _canExecute ? true : _canExecute();
     }
 
    void ICommand.Execute(object parameter)
     {
         _execute();
     }
}
```

The Command property of the button is assigned an instance of RelayCommand. This class constructor takes two parameters – an Action delegate called execute and a Func<bool> delegate called canExecute. The Action is called when the command is to be executed, whereas the Func<bool> determines if the command can be executed at all or not.

This is useful for performing runtime checks on the validity of input. The CanExecute delegate is triggered automatically by the framework periodically, which keeps the state of the button updated at all times.

The Command object should be made available through the view-model so that the button can be bound to it.

```csharp
public class CalculatorViewModel
{
    public CalculatorViewModel()
    {
        TryCalculateCommand = new RelayCommand(TryCalcuate, CanTryCalculate);
    }
 
    public ICommand TryCalculateCommand
     {
         get;
         private set;
     }
 
    private void TryCalcuate()
    {
    }
 
    private bool CanTryCalculate()
     {
         // Validate the input fields and return a boolean
     }
}
```

That’s it! All that’s left is to bind the Command property of the Button instance to the TryCalculateCommand property in the view-model.

```xml
<Button Content="Calculate" Command="{Binding TryCalculateCommand}"/>
```

Collectively, these minor improvements in paradigm make WPF a much nicer framework to use over its predecessors.
