   M S B A M L   `   `   `  ����E  AScoreboard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:Rubenhak.Common.WPFRubenhak.Common.WPF  5   	       WpfApplication2.SetTime   5   	   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   8x,http://schemas.microsoft.com/winfx/2006/xaml    5   	   5	commonWPF!clr-namespace:Rubenhak.Common.WPF    5   	     � Title$4  .Установка общего времени��5   	   $	��181��6*   $	��345��67    � WindowStartupLocation$ CenterScreen=�6C    � WindowStyle$ 
ToolWindow=�6h    � 
ResizeMode$ NoResize=�6�   .��5      � .+�5   
   �� -   5        $��MinutesText��5   L   $��83��6   $
��Left=�6   $��	12,27,0,0q�69   $	��Top=�6_   $	��120��6w   $��60(�6�   $��Center=�6�    �� 
IsReadOnly .  -   'Rubenhak.Common.WPF.TextBoxMaskBehavior   Mask$ Integer=�5         MinimumValue$ 0Q�5	         MaximumValue$ 10Q�5
      ��5   �   ��  �� ElementName$ MinutesSlider��	 �� Path$	 Value�5
      �� 5   
   -   5        $��MinutesSlider��5   L   $��81��6   $
��Left=�6   $��
138,29,0,0q�68   $	��Top=�6a   $��22��6y   
 �� Orientation$
 Vertical=�6�    �� TickFrequency$ 1Q�6�    �� TickPlacement$ TopLeft=�6�    � Maximum$ 10Q�6�    � SmallChange$ 1Q�6�    �� Interval$ 1��6�    � LargeChange$ 1Q�6�    �� IsSelectionRangeEnabled .   �� IsMoveToPointEnabled .   �� IsSnapToTickEnabled . 6<  �� 5   
   -   5        $��SecondsText��5   �   $��60(�6   $��83��6    $
��Left=�6,   $��Center=�6G    .  $��
166,27,0,0q�6~   $	��Top=�6�   $	��120��6�   $ Integer=�5      $ 0Q�5      $ 60Q�5      ��5   �   �� $ SecondsSlider��$	 Value�5      �� 5   
   -   5        $��SecondsSlider��5   �   $��81��6   $
��Left=�6   $ 1��68    .   .   . $ 1Q�6�   $��
292,29,0,0q�6�   $ 60Q�6�   $
 Vertical=�6�   $ 1Q�6�   $ 2Q�6
  $ TopLeft=�6  $	��Top=�64  $��22��6L  �� 5   
   -   5        $��button1��5   ^   $��Отмена��6   $��23��6"   $
��Left=�6.   $��239,118,0,0q�6I   $	��Top=�6m   $��75��6�    �� IsCancel . 6�   �� 5   
   -   5        $��button2��5   h   $%��Установить время��6   $��23��6,   $
��Left=�68   $��117,118,0,0q�6S   $	��Top=�6w   $	��116��6�   6�   �� 5   
   -   5        $��label1��5   [   $��Минуты:��6   $��28��6"   $
��Left=�6.   $��32,2,0,0q�6I   $	��Top=�6i   $��14(�6�   $
��Bold"�6�   �� 5   
   -   5        $��label2��5   ]   $��Секунды:��6   $��28��6#   $
��Left=�6/   $��	188,3,0,0q�6J   $	��Top=�6k   $��14(�6�   $
��Bold"�6�   5      5      