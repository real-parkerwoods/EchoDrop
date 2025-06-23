#!/bin/sh

cat << "END"
'||''''|       '||            '||'''|.                       
 ||   .         ||             ||   ||                       
 ||'''|  .|'',  ||''|, .|''|,  ||   || '||''| .|''|, '||''|, 
 ||      ||     ||  || ||  ||  ||   ||  ||    ||  ||  ||  || 
.||....| `|..' .||  || `|..|' .||...|' .||.   `|..|'  ||..|' 
                                                      ||     
                                                     .||     
|''||''| '\\  //`                                            
   ||      \\//                                              
   ||       ><                                               
   ||      //\\                                              
  .||.   .//  \\.                                            
                                                             
                                                             
END

while true; do
    echo
    read -p "Enter path to the file or directory you want to send: " input_path

    if [ ! -e "$input_path" ]; then
        echo "Error: Path not found or invalid characters."
        echo "Press [Enter] to try again or Ctrl+C to cancel."
        read dummy
        continue
    fi
    break
done

base_name=$(basename "$input_path")
file_ext="${base_name##*.}"
file_name="${base_name%.*}"

# Handle null extension
if [ "$file_ext" = "$file_name" ]; then
    file_ext=""
    file_name="$base_name"
fi

# Handle directories by archiving them
if [ -d "$input_path" ]; then
    archive_file="/tmp/${file_name}.tar"
    echo "Creating .tar archive..."
    tar -cf "$archive_file" -C "$(dirname "$input_path")" "$base_name"
    to_encode="$archive_file"
    file_ext="tar"
    file_name="${file_name}"
else
    to_encode="$input_path"
fi

echo "[DEBUG] to_encode: $to_encode"

# Verify the file actually exists
if [ ! -f "$to_encode" ]; then
    echo "[ERROR] File not prepared for encoding: '$to_encode'"
    echo "Check if tar or file preparation failed."
    exit 1
fi

# Determine encoder
if command -v base64 >/dev/null 2>&1; then
    ENCODER_CMD="base64"
    ENCODER_ARGS=""
elif command -v openssl >/dev/null 2>&1; then
    ENCODER_CMD="openssl"
    ENCODER_ARGS="base64 -A"
elif command -v uuencode >/dev/null 2>&1; then
    ENCODER_CMD="uuencode"
    ENCODER_ARGS=""
else
    echo "No base64-compatible encoder found (base64, openssl, uuencode)"
    echo "Press [Enter] to return to shell."
    read dummy
    exit 1
fi

echo
echo
echo "Before continuing:"
echo "   → ENABLE logging (Session > Logging > 'All session output' in PuTTY)"
echo
read -p "When ready, press any key to continue..." _
echo

echo "=== ECHODROP FILE BEGIN ==="
echo "FILENAME: $file_name"
echo "EXTENSION: $file_ext"
echo "ENCODING: $ENCODER_CMD"
echo "CONTENT:"

# Output encoded content with @ prefix
if [ "$ENCODER_CMD" = "base64" ]; then
    base64 < "$to_encode"
elif [ "$ENCODER_CMD" = "openssl" ]; then
    openssl base64 -in "$to_encode"
else
    uuencode "$to_encode" dummy.out | sed '1d;$d'
fi

echo "=== ECHODROP FILE END ==="
echo
