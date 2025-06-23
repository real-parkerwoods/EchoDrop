#!/bin/sh

# Fancy ASCII banner
cat << "END"


'||''''|       '||            '||'''|.                       
 ||   .         ||             ||   ||                       
 ||'''|  .|'',  ||''|, .|''|,  ||   || '||''| .|''|, '||''|, 
 ||      ||     ||  || ||  ||  ||   ||  ||    ||  ||  ||  || 
.||....| `|..' .||  || `|..|' .||...|' .||.   `|..|'  ||..|' 
                                                      ||     
                                                     .||     

END

# Get input path either from argument or prompt
while true; do
    if [ -z "$1" ]; then
        echo "Enter path to the file or directory you want to send:"
        read -r -p "> " input_path
    else
        input_path="$1"
    fi

    if [ ! -e "$input_path" ]; then
        echo "Error: Path not found or invalid characters."
        echo "Press [Enter] to try again or Ctrl+C to cancel."
        read dummy
        continue
    fi
    break
done

# Extract name and extension
base_name=$(basename "$input_path")
file_ext="${base_name##*.}"
file_name="${base_name%.*}"

# Handle null extension
if [ "$file_ext" = "$file_name" ]; then
    file_ext=""
    file_name="$base_name"
fi

# Archive directory to temp .tar if needed
archive_file="false"
tmp_file=""
if [ -d "$input_path" ]; then
    archive_file="true"
    file_ext="tar"
    tmp_file="/tmp/echodrop_$(date +%s%N).tar"
    tar -cf "$tmp_file" -C "$(dirname "$input_path")" "$(basename "$input_path")"
    to_encode="$tmp_file"
else
    to_encode="$input_path"
fi

# Verify file
if [ ! -f "$to_encode" ]; then
    echo "[ERROR] Could not locate or prepare file for encoding."
    [ -n "$tmp_file" ] && rm -f "$tmp_file"
    exit 1
fi

# Pick encoder
if command -v base64 >/dev/null 2>&1; then
    ENCODER_CMD="base64"
elif command -v openssl >/dev/null 2>&1; then
    ENCODER_CMD="openssl"
elif command -v uuencode >/dev/null 2>&1; then
    ENCODER_CMD="uuencode"
else
    echo "[ERROR] No encoder found (base64, openssl, uuencode)."
    [ -n "$tmp_file" ] && rm -f "$tmp_file"
    exit 1
fi

# Get checksum
checksum=$(sha256sum "$to_encode" | awk '{ print $1 }')

# Display instructions
echo
echo "Before continuing:"
echo "   → ENABLE logging (Session > Logging > 'All session output' in PuTTY)"
echo
read -p "When ready, press any key to continue..." _
echo

# File header
echo "{[=== ECHODROP FILE BEGIN ===]}"
echo "{[FILENAME: $file_name]}"
echo "{[EXTENSION: $file_ext]}"
echo "{[ENCODING: $ENCODER_CMD]}"
echo "{[CHECKSUM: $checksum]}"
echo "{[CONTENT:]}"

# Encode content and wrap with {[ ]}
if [ "$ENCODER_CMD" = "base64" ]; then
    base64 < "$to_encode" | sed -e 's/^/{[/' -e 's/$/]}/'
elif [ "$ENCODER_CMD" = "openssl" ]; then
    openssl base64 -in "$to_encode" | sed -e 's/^/{[/' -e 's/$/]}/'
else # uuencode
    uuencode "$to_encode" dummy.out | sed '1d;$d' | sed -e 's/^/{[/' -e 's/$/]}/'
fi

# File footer
echo "{[=== ECHODROP FILE END ===]}"

# Clean up any temp archive
[ -n "$tmp_file" ] && rm -f "$tmp_file"
