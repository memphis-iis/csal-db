"""single_turn.py

Super-simple script to post a single turn from a file

The command line is:

    python single_turn.py [-u <url>] <filename>
    
    -u <url> allows you to specify an alternate URL. If not specified,
       then $DEFAULT_URL will be used
    
    <filename> will be read and posted and so should be in JSON format

    Example to post ex1.json to $DEFAULT_URL:
    python single_turn.py ex1.json
    
    Example to post ex2.json to production:
    python single_turn.py -u http://ace.autotutor.com/csaldb/api/turn ex1.json
"""

import sys
import urllib2

DEFAULT_URL = "http://localhost:62702/api/turn"

def usage(titleline="Unknown Command"):
    print ""
    print titleline
    print '=' * len(titleline)
    print __doc__.replace("$DEFAULT_URL", DEFAULT_URL)
    return 1


def do_post(url, payload):
    print "URL: %s" % url
    print "Payload size: %d" % len(payload)
    
    #Actual post
    req = urllib2.Request(
        url, 
        payload, 
        {'Content-Type': 'application/json'}
    )
    resptxt = urllib2.urlopen(req).read()
    if resptxt:
        print "RESPONSE: %s" % resptxt
    
    print "DONE"
    print

def main():
    args = sys.argv[1:]
    
    if len(args) == 3 and args[0] == "-u":
        url, filename = args[1], args[2]
    elif len(args) == 1:
        url, filename = DEFAULT_URL, args[0]
    else:
        return usage()
    
    if not url:
        return usage("Missing URL?")
    if not filename:
        return usage("Missing filename?")

    with open(filename, "r") as inp:
        payload = inp.read()
    if not payload:
        return usage("File was empty?")
    
    #OK, everything appears to be OK
    do_post(url, payload)

if __name__ == "__main__":
    main()
