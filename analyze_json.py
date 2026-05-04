import json
import sys

try:
    with open('vswhere_output.json', 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Remove BOM if present
    if content.startswith('\ufeff'):
        content = content[1:]
        
    print(f"Content length: {len(content)}")
    
    # Try to parse with standard python json
    data = json.loads(content)
    print("Python json.loads: SUCCESS")
    
    # Wrap like Unity does
    wrapped = '{ "entries": ' + content + ' }'
    
    # Look for backslashes followed by invalid escape characters
    # JSON valid escapes: \", \\, \/, \b, \f, \n, \r, \t, \uXXXX
    i = 0
    while i < len(wrapped):
        if wrapped[i] == '\\':
            if i + 1 < len(wrapped):
                next_char = wrapped[i+1]
                if next_char not in ['"', '\\', '/', 'b', 'f', 'n', 'r', 't', 'u']:
                    print(f"INVALID ESCAPE at index {i}: \\{next_char} (hex: {hex(ord(next_char))})")
                    # Show context
                    start = max(0, i - 20)
                    end = min(len(wrapped), i + 20)
                    print(f"Context: ...{wrapped[start:end]}...")
            else:
                print(f"TRAILING BACKSLASH at index {i}")
        i += 1

except Exception as e:
    print(f"Error: {e}")
