   M S B A M L   `   `   `  ����   
HostLocker    HostLocker.RegisterWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     >�Name$  MainWindow1��.��� -   $��	main_grid��$��0,0,-6.4,3.4q�$y�Visible=�
������� � RowDefinitions �� $	��15* ��� $
��127* � � ColumnDefinitions �� $
��292* ��� $	��31* ��� $
��319* ��� $
��148* �.+��� -   $��btn_find��$��89,251.2,36.833,0q�$��23��$	��Top=�$��1��$y�Hidden=�.��Find Bluetooth Devices� -   $��pb��$��26��$��61,206.2,18.833,0q�$	��Top=� � IsIndeterminate . $y�Hidden=�$��1���� -   $��device_list��$��61,18.2,0,0q�$
��Left=�$	��224��$	��156��$	��Top=�$��1���� -   $��label1��$��28��$��104.6,86.2,110.6,0q�$	��Top=�$��2��$��1��.��Device Name:�� -   $��label2��$��28��$��Right=�$��0,126.2,131.6,0q�$	��Top=�$��73��$��2��$��1��.��
Connected:�� -   $��label3��$��28��$��104.6,159.2,111.6,0q�$	��Top=�$��2��$��1��.��Authenticated:�� -	   $��label4��$��Right=�$��0,192.2,116.6,0q�$��88��$��25��$	��Top=�$��2��$��1��.��Remembered:�� -
   $��label8��$��Right=�$��0,226.2,140.6,0q�$��64��$��25��$	��Top=�$��2��$��1��.��	LastUsed:�� -   $��txt_devicename��$��23��$��Right=�$��0,91.2,104,0q�$	��Top=�$	��150�� �� 
IsReadOnly . $��2��$��2��$��1���� -   $��txt_connected��$��23��$��Right=�$��0,123.2,104,0q�$	��Top=�$	��150�� . $��2��$��1��$��2���� -   $��txt_remembered��$��Right=�$��0,192.2,104,0q�$	��150��$��25��$	��Top=� . $��2��$��1��$��2���� -   $��txt_authenticated��$��Right=�$��0,160.2,103,0q�$	��150��$��21��$	��Top=� . $��2��$��1�� J� RenderTransformOrigin$ 0.5,0.5&�$��2��.��z�V� .~��� �� ��  �� Angle$ -0.06Q�T� .���� -   $��txt_lastused��$��Right=�$��0,228.2,104,0q�$	��150��$��23��$	��Top=� . $��2��$��1��$��2���� -   $��connect_to_device��$��Connect��$
��Left=�$��135,336.2,0,0q�$	��Top=�$��75��$y�Hidden=�$��1��$��19���� -   $��
listen_btn��$��Listen��$
��Left=�$��61,21,0,0.8q�$��85��$ -0.701,0.658&�$y�Visible=��� -   $��stop_listen_btn��$
��Stop��$
��Left=�$��27��$��
208,21,0,0q�$	��Top=�$��77��$y�Hidden=�$ 	1.58,0.44&�$��2���� -   $��QrCodeImage��$
��Left=�$	��450��$	��Top=�$	��450��$��
0.4,21,0,0q�$��2��$y�Visible=�$��2��$��2���� -   $��success_txt��$��2��$
��Left=�$��23��$��219.8,251.2,0,0q�$��1��$
��Wrap=�$��Registred and Connected��$	��Top=�$	��145��$��2��$y�Hidden=�