"""turn_copy.py

This handy script will either copy the turns out of the local MongoDB to a
file *OR* copy those files contents into the local server.

You must run on the command line with either the -i or -o option:

    -i <file-name> <url> will read the given input file and posted to the url
    -o <file-name> will read the local db and write to the given output file
"""

import sys
import json
import urllib2

import pymongo

def usage():
    print __doc__
    return 1


MONGO_URL = "mongodb://localhost:27017/csaldata"
MONGO_COLL = "studentActions"


def get_mongo():
    global MONGO_URL, MONGO_COLL
    client = pymongo.MongoClient(MONGO_URL)
    db = client.get_default_database()
    coll = db[MONGO_COLL]
    return client, db, coll


def do_input(filename, url):
    print "Reading: %s" % filename
    print "Target:  %s" % url
    
    #Make sure we can read everything before posting
    turns = []
    with open(filename, "r") as inp:
        for line in inp:
            if line.strip():
                turns.append(json.loads(line))
    print "Turns:   %d" % len(turns)
    
    for turn in turns:
        user = turn.get("UserID", "{MISSING}")
        lesson = turn.get("LessonID", "{MISSING}")
        
        evtInput = turn.get("Input", {})
        if evtInput:
            evt = evtInput.get("Event", "{BLANK}")
        else:
            evt = "{NOINPUT}"
        
        turnid = turn.get("TurnID", -1)
        print "  %s:%s:%d:%s" % (user, lesson, turnid, evt)

        #Actual post
        req = urllib2.Request(
            url, 
            json.dumps(turn), 
            {'Content-Type': 'application/json'}
        )
        resptxt = urllib2.urlopen(req).read()
        if resptxt:
            print "RESPONSE: %s" % resptxt


def do_output(filename):
    print "Reading: %s (%s)" % (MONGO_URL, MONGO_COLL)
    print "Target:  %s" % filename
    
    client, db, coll = get_mongo()
 
    with open(filename, "w") as out:
        for studentActions in coll.find():
            user = studentActions.get("UserID", "{MISSING}")
            lesson = studentActions.get("LessonID", "{MISSING}")
            print "User:%s Lesson:%s" % (user, lesson)
            for turn in studentActions.get("Turns", []):
                if not turn:
                    continue
                evtInput = turn.get("Input", {})
                if evtInput:
                    evt = evtInput.get("Event", "{BLANK}")
                else:
                    evt = "{NOINPUT}"
                turnid = turn.get("TurnID", -1)
                print "  %s:%s:%d:%s" % (user, lesson, turnid, evt)
                out.write(json.dumps(turn))
                out.write('\n')

def main():
    args = sys.argv[1:]
    
    if len(args) == 3 and args[0] == "-i":
        do_input(args[1], args[2])
    elif len(args) == 2 and args[0] == "-o":
        do_output(args[1])
    else:
        return usage()

if __name__ == "__main__":
    main()
