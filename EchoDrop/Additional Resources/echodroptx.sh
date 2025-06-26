#!/bin/bash
cat << "END"

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ ________           __                  _______                                @
@|        \         |  \                |       \                               @
@| $$$$$$$$ _______ | $$____    ______  | $$$$$$$\  ______    ______    ______  @
@| $$__    /       \| $$    \  /      \ | $$  | $$ /      \  /      \  /      \ @
@| $$  \  |  $$$$$$$| $$$$$$$\|  $$$$$$\| $$  | $$|  $$$$$$\|  $$$$$$\|  $$$$$$\@
@| $$$$$  | $$      | $$  | $$| $$  | $$| $$  | $$| $$   \$$| $$  | $$| $$  | $$@
@| $$_____| $$_____ | $$  | $$| $$__/ $$| $$__/ $$| $$      | $$__/ $$| $$__/ $$@
@| $$     \\$$     \| $$  | $$ \$$    $$| $$    $$| $$       \$$    $$| $$    $$@
@ \$$$$$$$$ \$$$$$$$ \$$   \$$  \$$$$$$  \$$$$$$$  \$$        \$$$$$$ | $$$$$$$ @
@                                                                     | $$      @
@                                                                     | $$      @
@                                                                      \$$      @
@                                                 ________  __    __            @
@                                                |        \|  \  |  \           @
@                                                 \$$$$$$$$| $$  | $$           @
@                                                   | $$    \$$\/  $$           @
@                                                   | $$     >$$  $$            @
@                                                   | $$    /  $$$$\            @
@                                                   | $$   |  $$ \$$\           @
@                                                   | $$   | $$  | $$           @
@                                                    \$$    \$$   \$$           @
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

END
delimiter=""

# Parse flags
while getopts ":d:" opt; do
  case $opt in
    d)
      if [ -z "$OPTARG" ]; then
        echo "Error: -d requires a non-empty argument"
        exit 1
      fi
      delimiter="$OPTARG"
      ;;
    \?)
      echo "Invalid option: -$OPTARG"
      exit 1
      ;;
    :)
      echo "Option -$OPTARG requires an argument"
      exit 1
      ;;
  esac
done
shift $((OPTIND -1))

# Prompt until valid file or directory path is entered
while true; do
    if [ -z "$1" ]; then
        echo "Enter path to the file or directory you want to send:"
        read -r -p "> " input_path
    else
        input_path="$1"
        shift
    fi

    if [ ! -e "$input_path" ]; then
        echo "Error: Path not found or invalid characters."
        echo "Press [Enter] to try again or Ctrl+C to cancel."
        read dummy
    else
        break
    fi
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
    trap 'rm -f "$tmp_file"; exit' INT TERM
    to_encode="$tmp_file"
else
    archive_file="false"
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
if [ "$archive_file" = "true" ]; then
    checksum=$(sha256sum "$tmp_file" | awk '{ print $1 }')
else
    checksum=$(sha256sum "$to_encode" | awk '{ print $1 }')
fi

# Display instructions
echo
echo "Before continuing:"
echo "   → ENABLE logging (Session > Logging > 'All session output' in PuTTY)"
echo
read -p "When ready, press any key to continue..." _
echo

# File header
echo "${delimiter}=== ECHODROP FILE BEGIN ==="
echo "${delimiter}FILENAME: $file_name"
echo "${delimiter}EXTENSION: $file_ext"
echo "${delimiter}ENCODING: $ENCODER_CMD"
echo "${delimiter}CHECKSUM: $checksum"
echo "${delimiter}CONTENT:"

if [ "$ENCODER_CMD" = "base64" ]; then
    base64 < "$to_encode" | sed -e "s/^/${delimiter}/"
elif [ "$ENCODER_CMD" = "openssl" ]; then
    openssl base64 -in "$to_encode" | sed -e "s/^/${delimiter}/"
else # uuencode
    uuencode "$to_encode" dummy.out | sed '1d;$d' | sed -e "s/^/${delimiter}/"
fi

# File footer
echo "${delimiter}=== ECHODROP FILE END ==="

# Clean up any temp archive
[ -n "$tmp_file" ] && rm -f "$tmp_file"
