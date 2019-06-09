#!/bin/bash

HIP_DIR="/home/$USER/.paradoxinteractive/Crusader Kings II/mod/Historical Immersion Project"
TEMP_FILE="./.localisations.temp.csv"
FINAL_FILE="./localisations.csv"

if [ ! -d "$HIP_DIR" ]; then
    echo "PLEASE EDIT THIS FILE WITH THE CORRECT HIP PATH"
    exit 1
fi

if [ -f "$TEMP_FILE" ]; then
    rm "$TEMP_FILE"
    touch "$TEMP_FILE"
fi

function bring_localisation_file {
    LOCALISATION_FILE="$1"
    LANG="en_US.iso88591"

    cat "$LOCALISATION_FILE" | grep -av "\(^#.*\|^[^;]*_[Aa][Dd][Jj][;_ ].*\|^.[^_].*\|^[^bcdkeBCDKE].*\)" | sed 's/^\([^;]*\);\([^;]*\);.*$/\1,\2/g' >> "$TEMP_FILE"
}

function bring_provinces {
    for FILE in "$HIP_DIR"/history/provinces/*.txt; do
        LANG="en_US.iso88591"
        NUMERICAL_ID=$(echo "$FILE" | sed 's|.*/\([0-9]*\).*|\1|g')
        TITLE_ID=$(cat "$FILE" | grep -a "title\s*=\s*" | sed 's/\s//g' | sed 's/#.*//' | sed 's/^title=\(.*\)$/\1/g')
        LOCALISATION=$(cat "$HIP_DIR/localisation/A_SWMHprovinces.csv" | grep -a "^\s*PROV${NUMERICAL_ID};" | sed 's/^\s*[^;]*;\([^;]*\);.*/\1/g')

        if [ -z "$LOCALISATION" ]; then
            LOCALISATION=$(cat "$FILE" | grep -a "^\s*#\s*[0-9]*\s*-\s*[^\*]" | sed 's/\s*#\s*[0-9]*\s*-\s*\(.*\)/\1/g' | sed 's/\((.*)\|\/.*$\)//g' | grep -av "#")
        fi

        if [ -z "$LOCALISATION" ]; then
            LOCALISATION=$(echo "$FILE" | sed 's|.*/[0-9]*\s*-\s*\(.*\)\.txt|\1|g')
        fi

        LOCALISATION=$(echo "$LOCALISATION" | sed 's/\(^\s*\|\s*$\)//')

        echo "$TITLE_ID,$LOCALISATION" >> "$TEMP_FILE"
    done
}

bring_localisation_file "$HIP_DIR/localisation/1_emf_titles.csv"
bring_localisation_file "$HIP_DIR/localisation/1_PB_new_titles.csv"
bring_localisation_file "$HIP_DIR/localisation/A_SWMHbaronies.csv"
bring_localisation_file "$HIP_DIR/localisation/A_DuchiesKingdomsandEmpires_de_jure.csv"
bring_localisation_file "$HIP_DIR/localisation/A_DuchiesKingdomsandEmpires_titular.csv"
bring_provinces

iconv -f WINDOWS-1252 -t UTF-8 "$TEMP_FILE" > "$TEMP_FILE.2"
cat "$TEMP_FILE.2" | sort -k1,1 -u --field-separator=, | uniq > "$TEMP_FILE"

mv "$TEMP_FILE" "$FINAL_FILE"
