using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;


[System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct |
                       System.AttributeTargets.Field)
]
public class ExposeDebugConsole : System.Attribute
{
     public ExposeDebugConsole()
    {
    }
}

public partial class DebugConsole : Control
{
    private class InputCommand
    {
        public string instance;
        public string command;
        public RegisteredMethod method;
        public Variant[] args;
    }

    private class ClassTypes<T>
    {
        public string typeName;
        public T type;
    }

    private class WatchList
    {
        public string instance;
        public string field;
        public int line;
        public float updateTime;

        public float currentTime;
    }



    public delegate void RegisteredMethod(Variant[] args);


    private RegisteredMethod regMethod;
    // Nodes
    private AnimationPlayer animationPlayer;
    
    
    private RichTextLabel textEdit;
    private RichTextLabel watchListEdit;
    private LineEdit commandInput;
    private Panel rootPanel;
    private Fields commandWindow;
    private Fields instanceWindow;
    private Fields fieldsWindow;
    private Fields methodsWindow;
    private Sprite2D attention;

    // Vars
    private bool isActive = false;
    private bool isAllAccess = false;

    private List<InputCommand> inputCommands = new List<InputCommand>();

    private List<string> commandList = new List<string>();
    private int currentCommand = 0;


    private readonly List<string> commands = new List<string>() { "set","get","call","exit","reload","watch" };
    private List<string> instanceList;
    private List<WatchList> watchList = new List<WatchList>();

    private string currentSelectedInstance;

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        commandInput = GetNode<LineEdit>("Panel/VBoxContainer/LineEdit");
        textEdit = GetNode<RichTextLabel>("Panel/VBoxContainer/HBoxContainer/TextEdit");
        watchListEdit = GetNode<RichTextLabel>("Panel/VBoxContainer/HBoxContainer/WatchList");
        fieldsWindow = GetNode<Fields>("Field");
        commandWindow = GetNode<Fields>("Command");
        instanceWindow = GetNode<Fields>("Instances");
        methodsWindow = GetNode<Fields>("Methods");
        attention = GetNode<Sprite2D>("Panel/Attention");
        attention.Visible = false;

        rootPanel = GetNode<Panel>("Panel");
        rootPanel.Position = new Vector2(0, -500);
        isActive = false;
        Visible = false;
        isAllAccess = false;
        instanceList = new List<string>();

        Assembly asm = Assembly.GetExecutingAssembly();

        foreach (Type type in asm.GetTypes())
            if (Attribute.IsDefined(type,typeof(ExposeDebugConsole)))
                instanceList.Add(type.Name);
        Globals.Instance.SetDebugConsole(this);    
    }

    public void _on_line_edit_on_all_access_toggle  ()
    {
        isAllAccess = !isAllAccess;
        attention.Visible = isAllAccess;
    }

    public void Show()
    {
        isActive = true;
        animationPlayer.Play("fade_in");
        Visible = true;

        Input.MouseMode = Input.MouseModeEnum.Visible;
        commandInput.GrabFocus();
    }

    public void Hide()
    {
        animationPlayer.Play("fade_out");
        isActive = false;
        Visible = false;

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public void Toggle()
    {
        if (isActive)
            Hide();
        else
            Show();
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void _on_line_edit_on_command_enter()
    {
        commandList.Add(commandInput.Text);
        currentCommand = commandList.Count();
        PrintMessage(commandInput.Text);
        commandInput.Text = "";
    }

    public void _on_line_edit_on_help_pressed()
    {
        commandWindow.AddNewList(commands);
        commandWindow.Popup();

    }

    public void PrintMessage(string message)
    {
        textEdit.Text += message;
        DecipherInput(message);
        textEdit.Text += "\n";

        commandInput.Text = "";
    }

    public void RegisterMethod(string instance,string command,RegisteredMethod method)
    {
        InputCommand com = new InputCommand();
        com.instance = instance;
        com.command = command;
        com.method = method;
        inputCommands.Add(com);
    }

    private void DecipherInput(string input)
    {
        string[] words = input.Split(' ',20,StringSplitOptions.RemoveEmptyEntries);


        switch (words[0])
        {
            // 0    1      2             3
            // set player inputResponse 0.4
            case "set":
                if (words.Length>2)
                    SetInstanceValue(words[1],words[2],words[3]);
                break;

            //  0    1         2
            // get player inputResponse
            case "get":
                if (words.Length>1)
                    textEdit.Text += "  ==> " + GetInstanceValue(words[1],words[2]);
                break;

            case "test":
            {
                GD.Print("test");
                

                Assembly asm = Assembly.GetExecutingAssembly();

                
                foreach (Type type in asm.GetTypes())
                {
                    if (Attribute.IsDefined(type,typeof(ExposeDebugConsole)))
                    {
                        GD.Print(type.Name);
                    }
                }

                
            }
            break;
            case "call":
            {
                // get instance and method
                string command = words[1];  // instance.method
                // args = index->2...
                List<Variant> args = new List<Variant>();
                for (int i=2;i<words.Count();i++)
                    args.Add((Variant)words[i]);

                foreach (InputCommand ic in inputCommands)
                    if ((ic.instance+"."+ic.command)==command)
                    {
                        // call method
                        ic.method(args.ToArray());
                    }

            }
            break;

            //  0    1         2           3
            // watch player inputResponse 0.5
            case "watch":
            {
                if (words.Count()>2)
                {
                    WatchList wl = new WatchList();
                    wl.instance = words[1];
                    wl.field = words[2];
                    wl.updateTime = Convert.ToSingle(words[3]);
                    wl.line = textEdit.GetLineCount()-1;
                    wl.currentTime = 0;
                    watchList.Add(wl);
                }
            }
            break;

            case "exit":
                GetTree().Quit();
            break;
            
            case "reload":
                GetTree().ReloadCurrentScene();
            break;

            default:
                // check for user commands
                foreach (InputCommand c in inputCommands)
                {
                    if (c.command == words[0])
                    {
                        // call method

                    }
                }
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Update Watchlist
        foreach(WatchList wl in watchList)
        {
            wl.currentTime -= (float)delta;
            if (wl.currentTime<=0)
                wl.currentTime = wl.updateTime;
            
        }
        watchListEdit.Text="";
        foreach(WatchList wl in watchList)
            watchListEdit.Text += wl.instance+"."+wl.field+" ==> "+GetInstanceValue(wl.instance,wl.field)+"\n";

    }
    
    
    
    private void SetInstanceValue(string instance,string field,string value)
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        foreach (Type type in asm.GetTypes())
        {
            if (Attribute.IsDefined(type,typeof(ExposeDebugConsole)))
            {
                GD.Print(type.Name);
                if (type.Name==instance)
                {
                    var p = GetTree().Root.FindChild(instance,true,false);
                    if (p!=null)
                    {
                        try
                        {
                            FieldInfo tp = type.GetField(field,System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            SetValue(tp,value,p);

                        } catch (Exception ex)
                        {
                            FieldInfo tp = type.GetField(field,System.Reflection.BindingFlags.Public |System.Reflection.BindingFlags.Instance);
                            SetValue(tp,value,p);
                        } 
                    } else
                    {
                        textEdit.Text += "\nCould not set field\n";
                    }
                }
            }
        }

    }

    private void SetValue(FieldInfo tp,string value,Object p)
    {
        if (tp.FieldType==typeof(System.String))
        {
            var val = value as string;
            tp.SetValue(p,val);
        } else if (tp.FieldType==typeof(float))
        {
            var val = Convert.ToSingle(value);
            tp.SetValue(p,val);
        } else if (tp.FieldType==typeof(int))
        {
            var val = Convert.ToInt64(value);
            tp.SetValue(p,val);

        } else if (tp.FieldType==typeof(bool))
        {
            var val = Convert.ToBoolean(value);
            tp.SetValue(p,val);
        }
    }


    private string GetInstanceValue(string instance,string field)
    {
        string result = "";

        Assembly asm = Assembly.GetExecutingAssembly();

        foreach (Type type in asm.GetTypes())
        {
            if (Attribute.IsDefined(type,typeof(ExposeDebugConsole)))
            {
                if (type.Name==instance)
                {
                    var p = GetTree().Root.FindChild(instance,true,false);
                    if (p!=null)
                    {
                        try
                        {
                            FieldInfo tp = type.GetField(field,System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            result = tp.GetValue(p).ToString();
                        } catch (Exception ex)
                        {
                            FieldInfo tp = type.GetField(field,System.Reflection.BindingFlags.Public |System.Reflection.BindingFlags.Instance);
                            result = tp.GetValue(p).ToString();
                        } 
                    } else
                    {
                        result = "Instance or Field not found";
                    }
                }
            }
        }
        return result;        
    }

    public void _on_line_edit_on_command_down()
    {
        if (currentCommand<commandList.Count-1)
        {
            currentCommand++;
            commandInput.Text = commandList[currentCommand];
        }
    }

    public void _on_line_edit_on_command_up()
    {
        if (currentCommand>0)
        {
            currentCommand--;
            commandInput.Text = commandList[currentCommand];
        }
    }

    private void PopupInstanceWindow()
    {
        instanceWindow.AddNewList(instanceList);
        instanceWindow.Popup();
    }

    private void PopupFieldsWindow(string instance)
    {
        fieldsWindow.AddNewList(getFieldsFromInstance(instance));
        fieldsWindow.Popup();
    }


    public void _on_command_close_requested()
    {
        commandWindow.Hide();
        AddCommandFromList(commandWindow.getCurrentIndex());
    }

    public void _on_instances_close_requested()
    {
        instanceWindow.Hide();
        AddInstanceFromList(instanceWindow.getCurrentIndex());
    }

    public void _on_field_close_requested()
    {
        fieldsWindow.Hide();
        AddFieldFromList(fieldsWindow.getCurrentIndex());
    }

    public void _on_command_on_item_selected(int index)
    {
        commandWindow.Hide();
        AddCommandFromList(index);
        
    }

    public void _on_instances_on_item_selected(int index)
    {
        instanceWindow.Hide();
        AddInstanceFromList(index);

    }

    public void _on_field_on_item_selected(int index)
    {
         fieldsWindow.Hide();
         AddFieldFromList(index);
    }

    private void AddCommandFromList(int index)
    {
        commandInput.Text += commands[index] + " ";
        if ((commands[index]=="exit") || (commands[index]=="reload"))
            return;
        if (commands[index]=="call")
        {
            PopupMethodsWindow();
        } else
            PopupInstanceWindow();
    }

    private void AddInstanceFromList(int index)
    {
        currentSelectedInstance = instanceList[index];
        commandInput.Text += instanceList[index] + " ";
        PopupFieldsWindow(currentSelectedInstance);
    }

    private void AddFieldFromList(int index)
    {
        List<string> instanceFields = getFieldsFromInstance(currentSelectedInstance);
        commandInput.Text += instanceFields[index] + " ";
    }

    private List<string> getFieldsFromInstance(string instance)
    {
        List<string> result = new List<string>();

        Assembly asm = Assembly.GetExecutingAssembly();

        foreach (Type type in asm.GetTypes())
            if (type.Name==instance)
            {
                var p = GetTree().Root.FindChild(instance,true,false);
                if (p!=null)
                {
                    FieldInfo[] tp = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    foreach (FieldInfo tt in tp)
                    {

                        if ((Attribute.IsDefined(tt,typeof(ExposeDebugConsole))) && !isAllAccess)
                            result.Add(tt.Name);
                        else if (isAllAccess)
                            result.Add(tt.Name);
                    }                    
                }
            }
        return result;
    }

    private void PopupMethodsWindow()
    {
        List<string> methodList = new List<string>();
        foreach (InputCommand ic in inputCommands)
            methodList.Add(ic.instance+"."+ic.command);
        methodsWindow.AddNewList(methodList);
        methodsWindow.Popup();
    }

    private void _on_methods_close_requested()
    {

        commandInput.Text += inputCommands[methodsWindow.getCurrentIndex()].instance+"."+inputCommands[methodsWindow.getCurrentIndex()].command;
        methodsWindow.Hide();
    }

    private void _on_methods_on_item_selected(int index)
    {

        commandInput.Text += inputCommands[index].instance+"."+inputCommands[index].command;
        methodsWindow.Hide();
    }
}
