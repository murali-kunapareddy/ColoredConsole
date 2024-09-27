# ColoredConsole.NET

A simple library that enhances console application user experience by provided colored console.
_Current Version: 1.0.5_
>&copy; 2023-2024. MIT Licensed.

## Getting Started
Follow the basic steps to get started
1. Add namespace
    ```sh
    using bcd;
    ```
2. Declare variable
    ```sh
    static ColoredConsole cc = new ColoredConsole();
    ```
3. Now use the features to beautify your console
    - To draw a box around the text like header:
        ```sh
        cc.DrawBox("This is test");
        ```
    - To write some text inside box with separators:
        ```sh
       cc.DrawTopLine();
       cc.WriteLine("Some text");
       cc.DrawSeparator();
       cc.WriteLine("This is body text");
       cc.DrawBottomLine();
        ```
## Change Log

|Date      |Version|Description                                 |
|----------|-----  |--------------------------------------------|
|2020-09-10|1.0.0  |Initial version						        |
|2023-02-20|1.0.1  |`DrawSeparator` updated				        |
|2023-02-21|1.0.2  |`Prompt` feature added				        |
|2023-03-15|1.0.3  |`Progressbar` feature added			        |
|2023-03-26|1.0.4  |**File logging** added				        |
|2024-09-24|1.0.5  |`ProgressBar` updated				        |
|          |       |`DrawSeparator` updated with Section Text   |
|          |       |`AutoNumber` feature added					|




