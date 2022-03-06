//
//  TextValidator.m
//  TouchScreenKeyboard
//
//  Created by Jeroen Pienbroek on 25/01/2018.
//  Copyright © 2018 Jeroen van Pienbroek. All rights reserved.
//

#import "NKBTextValidator.h"
#import <Foundation/Foundation.h>
#import "NKBUtil.h"
#import "NKBNSString+Extensions.h"

const NSString* EMAIL_SPECIAL_CHARACTERS = @"!#$%&'*+-/=?^_`{|}~";

@implementation NKBTextValidator

-(void)validate:(NSString*) text_
   textToAppend:(NSString*) textToAppend_
  caretPosition:(int) caretPosition_
selectionStartPosition:(int) selectionStartPosition_
{
    int textLength = (int)text_.length;
    int textToAppendLength = (int)textToAppend_.length;
    
    int startCaretPosition = caretPosition_;
    unichar buffer[textLength + textToAppendLength];
    [text_ getCharacters:buffer range:NSMakeRange(0, text_.length)];
    
    int position = 0;
    for(int i = 0; i < textToAppendLength; i++)
    {
        unichar ch = [textToAppend_ characterAtIndex:i];
        unichar result = [self validateChar:ch text:buffer textLength:position pos:position caretPosition:caretPosition_ selectionStartPosition:selectionStartPosition_];
        if(result != 0)
        {
            buffer[position] = result;
            position++;
        }
    }
    
    if(startCaretPosition < textLength)
    {
        for(int i = startCaretPosition; i < textLength; i++)
        {
            unichar ch = [text_ characterAtIndex:i];
            unichar result = [self validateChar:ch text:buffer textLength:position pos:position caretPosition:caretPosition_ selectionStartPosition:selectionStartPosition_];
            if(result != 0)
            {
                buffer[position] = result;
                position++;
            }
        }
    }
    
    textLength = position;
    
    _resultText = [NSString stringWithCharacters:buffer length:textLength];
    _resultCaretPosition = caretPosition_;
}

-(unichar)validateChar:(unichar) ch_
              text:(unichar*) text_
        textLength:(int) textLength_
               pos:(int) pos_
     caretPosition:(int) caretPosition_
selectionStartPosition:(int) selectionStartPosition_
{
    if(_lineType != LT_MULTI_LINE_NEWLINE && (ch_ == '\r' || ch_ == '\n'))
    {
        return (char)0;
    }
    
    // Validation is disabled
    if(_validation == CV_NONE)
    {
        return ch_;
    }
    
    if(_validation == CV_CUSTOM)
    {
        if(_validator == nil) { return ch_; }
        else
        {
            return [_validator validate:ch_ text:text_ textLength:textLength_ pos:pos_ selectionStartPosition:selectionStartPosition_];
        }
    }
    
    if(_validation == CV_INTEGER || _validation == CV_DECIMAL || _validation == CV_DECIMAL_FORCE_POINT)
    {
        // Integer and decimal
        bool cursorBeforeDash = (pos_ == 0 && textLength_ > 0 && text_[0] == '-');
        bool dashInSelection = textLength_ > 0 && text_[0] == '-' && ((caretPosition_ == 0 && selectionStartPosition_ > 0) || (selectionStartPosition_ == 0 && caretPosition_ > 0));
        bool selectionAtStart = caretPosition_ == 0 || selectionStartPosition_ == 0;
        if(!cursorBeforeDash || dashInSelection)
        {
            if(ch_ >= '0' && ch_ <= '9') return ch_;
            if(ch_ == '-' && (pos_ == 0 || selectionAtStart)) return ch_;
            if(_validation == CV_DECIMAL)
            {
                if(ch_ == '.' || ch_ == ',')
                {
                    if(![NKBUtil contains:'.' text:text_ textLength:textLength_] && ![NKBUtil contains:',' text:text_ textLength:textLength_]) return ch_;
                }
            }
            else if(_validation == CV_DECIMAL_FORCE_POINT)
            {
                if(ch_ == '.' && ![NKBUtil contains:'.' text:text_ textLength:textLength_]) return ch_;
                if(ch_ == ',' && ![NKBUtil contains:'.' text:text_ textLength:textLength_]) return '.';
            }
        }
    }
    else if(_validation == CV_ALPHANUMERIC)
    {
        // All alphanumeric characters
        if(ch_ >= 'A' && ch_ <= 'Z') return ch_;
        if(ch_ >= 'a' && ch_ <= 'z') return ch_;
        if(ch_ >= '0' && ch_ <= '9') return ch_;
    }
    else if(_validation == CV_NAME)
    {
        // FIXME: some actions still lead to invalid input:
        //        - Hitting delete in front of an uppercase letter
        //        - Selecting an uppercase letter and deleting it
        //        - Typing some text, hitting Home and typing more text (we then have an uppercase letter in the middle of a word)
        //        - Typing some text, hitting Home and typing a space (we then have a leading space)
        //        - Erasing a space between two words (we then have an uppercase letter in the middle of a word)
        //        - We accept a trailing space
        //        - We accept the insertion of a space between two lowercase letters.
        //        - Typing text in front of an existing uppercase letter
        //        - ... and certainly more
        //
        // The rule we try to implement are too complex for this kind of verification.
        bool isLetter = [[NSCharacterSet letterCharacterSet] characterIsMember: ch_] && (toupper(ch_) != tolower(ch_));
        if(isLetter)
        {
            // Character following a space should be in uppercase.
            bool isLower = [[NSCharacterSet lowercaseLetterCharacterSet] characterIsMember: ch_];
            if(isLower && ((pos_ == 0) || text_[pos_ - 1] == ' '))
            {
                return toupper(ch_);
            }
            
            // Character not following a space or an apostrophe should be in lowercase.
            bool isUpper = [[NSCharacterSet uppercaseLetterCharacterSet] characterIsMember: ch_];
            if(isUpper && (pos_ > 0) && text_[pos_ - 1] != ' ' && text_[pos_ - 1] != '\'')
            {
                return tolower(ch_);
            }
            
            return ch_;
        }
        
        if(ch_ == '\'')
        {
            // Don't allow more than one apostrophe
            if(![NKBUtil contains:'\'' text:text_ textLength:textLength_])
            {
                // Don't allow consecutive spaces and apostrophes.
                if(!(((pos_ > 0) && ((text_[pos_ - 1] == ' ') || (text_[pos_ - 1] == '\''))) ||
                     ((pos_ < textLength_) && ((text_[pos_] == ' ') || (text_[pos_] == '\'')))))
                {
                    return ch_;
                }
            }
        }
        
        if(ch_ == ' ')
        {
            // Don't allow consecutive spaces and apostrophes.
            if(!(((pos_ > 0) && ((text_[pos_ - 1] == ' ') || (text_[pos_ - 1] == '\''))) ||
                 ((pos_ < textLength_) && ((text_[pos_] == ' ') || (text_[pos_] == '\'')))))
            {
                return ch_;
            }
        }
    }
    else if(_validation == CV_EMAIL_ADDRESS)
    {
        // From StackOverflow about allowed characters in email addresses:
        // Uppercase and lowercase English letters (a-z, A-Z)
        // Digits 0 to 9
        // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
        // Character . (dot, period, full stop) provided that it is not the first or last character,
        // and provided also that it does not appear two or more times consecutively.
        
        if([[NSCharacterSet letterCharacterSet] characterIsMember:ch_]) return ch_;
        if(ch_ >= '0' && ch_ <= '9') return ch_;
        if(ch_ == '@' && [NKBUtil indexOf:'@' text:text_ textLength:textLength_] == -1) return ch_;
        if([EMAIL_SPECIAL_CHARACTERS indexOf:ch_] != -1) return ch_;
        if(ch_ == '.')
        {
            unichar lastChar = 0;
            if(textLength_ > 0)
            {
                int clampedIndex = (int)[NKBUtil clamp:pos_ min:0 max:textLength_ - 1];
                lastChar = text_[clampedIndex];
            }
            else
            {
                lastChar = ' ';
            }
            
            unichar nextChar = 0;
            if(textLength_ > 0)
            {
                int clampedIndex = (int)[NKBUtil clamp:pos_ + 1 min:0 max:textLength_ - 1];
                nextChar = text_[clampedIndex];
            }
            else
            {
                nextChar = '\n';
            }
            
            if(lastChar != '.' && nextChar != '.')
            {
                return ch_;
            }
        }
    }
    else if(_validation == CV_IP_ADDRESS)
    {
        int lastDotIndex = [NKBUtil lastIndexOf:'.' text:text_ textLength:textLength_];
        if(lastDotIndex == -1)
        {
            int numbersInSection = textLength_;
            if(numbersInSection < 3 && ch_ >= '0' && ch_ <= '9') return ch_; //Less than 3 numbers, so number add allowed
            if(ch_ == '.' && textLength_ > 0) { return ch_; } //Don't start with dot
        }
        else
        {
            if(ch_ >= '0' && ch_ <= '9')
            {
                int numbersInSection = (textLength_ - 1) - lastDotIndex;
                if(numbersInSection < 3 && ch_ >= '0' && ch_ <= '9') return ch_; //Less than 3 numbers, so number add allowed
            }
            if(ch_ == '.' && lastDotIndex != textLength_ - 1 && [NKBUtil countOccurences:'.' text:text_ textLength:textLength_] < 3) { return ch_; } //Max 4 sections (3 dot characters)
        }
    }
    else if(_validation == CV_SENTENCE)
    {
        bool isLetter = [[NSCharacterSet letterCharacterSet] characterIsMember: ch_] && (toupper(ch_) != tolower(ch_));
        bool isLower = [[NSCharacterSet lowercaseLetterCharacterSet] characterIsMember: ch_];
        if(isLetter && isLower)
        {
            if(pos_ == 0) { return toupper(ch_); }
            
            if(pos_ > 1 && text_[pos_ - 1] == ' ' && text_[pos_ - 2] == '.')
            {
                return toupper(ch_);
            }
        }
        
        return ch_;
    }
    return (unichar)0;
}

@end

