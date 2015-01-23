"""db_init.py

This hard-coded script uses the base http endpoint given to save some initial
classes, lessons, and students.  Currently they are just hard-coded in this
file.

You must run on the command line with a single parameter that is the api
endpoint to target.  Some examples:

    To target the local dev workstation:
    ./db_init.py http://localhost:62702/api

    To target the "prod" server:
    ./db_init.py http://ace.autotutor.com/csaldb/api

NOTE: classes, lessons, and students are read first and then updated with
current data so untouched fields will remain the same for already-populated
objects

NOTE: that we assume that all students are in all lessons and all lessons
are in all classes.  AS A RESULT, you should probably run this script once
per class and modify the data between classes.  Of course, A FAR BETTER
SOLUTION would be to change this script to accept a file per class
"""

import sys
import json
import urllib2

def usage():
    print __doc__
    return 1

#Classes are (location, name/id, teacher, meet time)
CLASSES = [
    ("Memphis", "testclass", "whitney.baer@gmail.com", "Tue 10am"),
]

#Lessons are (id, shortname)
LESSONS = [
    ("lesson0", "0. Introduction"),
    ("lesson1", "1. Text Signals"),
    ("lesson2", "2. Writer's Purpose"),
    ("lesson3", "3. Hybrid Texts"),
    ("lesson4", "4. Affixes"),
    ("lesson5", "5. Punctuation"),
    ("lesson6", "6. Context Clues"),
    ("lesson7", "7. Acquiring New Words"),
    ("lesson8", "8. Multiple Meaning Words"),
    ("lesson9", "9. Pronouns"),
    ("lesson10", "10. Non-Literal Language"),
    ("lesson11", "11. Review"),
    ("lesson12", "12. Using Key Information"),
    ("lesson13", "13. Questioning: Narrative"),
    ("lesson14", "14. Bridge Building"),
    ("lesson15", "15. Summarizing Narrative"),
    ("lesson16", "16. Questioning: Informational"),
    ("lesson17", "17. Questioning: Persuasive"),
    ("lesson18", "18. Review"),
    ("lesson19", "19. Statement and Explanation"),
    ("lesson20", "20. Problem Solution"),
    ("lesson21", "21. Cause and Effect"),
    ("lesson22", "22. Description and Spatial"),
    ("lesson23", "23. Compare and Contrast"),
    ("lesson24", "24. Time Order"),
    ("lesson25", "25. Procedural"),
    ("lesson26", "26. Review"),
    ("lesson27", "27. Elaborating: Narrative"),
    ("lesson28", "28: Elaborating: Informative"),
    ("lesson29", "29. Elaborating: Persuasive"),
    ("lesson30", "30. Documents"),
]

#Students are (id, first name, last name)
STUDENTS = [
    ("craig", "Craig", "Kelly"),
    ("whitney", "Whitney", "Baer"),
]

def do_get(url):
    #Actual get (read)
    req = urllib2.Request(url)
    resptxt = urllib2.urlopen(req).read()
    if not resptxt:
        return None #Nothing found
    return json.loads(resptxt)

def do_post(url, data):
    #Actual post (write)
    req = urllib2.Request(
        url,
        json.dumps(data),
        {'Content-Type': 'application/json'}
    )
    resptxt = urllib2.urlopen(req).read()
    if resptxt:
        print "FOR REQ: %s %s" % (url, str(data))
        print "RESPONSE: %s" % resptxt
        raise ValueError("Response from server?!")

def list_merge(lst1, lst2):
    return sorted(set(lst1).union(set(lst2)))

def one_class(base, cls, lessons, students):
    loc, clsid, teacher, meet = cls
    url = "%s/classes/%s" % (base, clsid)

    data = do_get(url)
    if data:
        print "Update Class %s" % clsid
    else:
        print "CREATE Class %s" % clsid
        data = {}
        data["_id"] = clsid
        data["ClassID"] = clsid
        data["Students"] = []
        data["Lessons"] = []
        data["AutoCreated"] = False

    data["TeacherName"] = teacher
    data["Location"] = loc
    data["MeetingTime"] = meet
    data["Students"] = list_merge(data["Students"], students)
    data["Lessons"] = list_merge(data["Lessons"], lessons)

    do_post(url, data)

def one_lesson(base, lesson, students):
    lid, shortname = lesson
    url = "%s/lessons/%s" % (base, lid)

    data = do_get(url)
    if data:
        print "Update Lesson %s" % lid
    else:
        print "CREATE Lesson %s" % lid
        data = {}
        data["_id"] = lid
        data["LessonID"] = lid
        data["TurnCount"] = 0
        data["AttemptTimes"] = []
        data["Students"] = []
        data["StudentsAttempted"] = []
        data["StudentsCompleted"] = []
        data["URLs"] = []
        data["AutoCreated"] = False

    data["ShortName"] = shortname
    data["Students"] = list_merge(data["Students"], students)

    do_post(url, data)

def one_student(base, student):
    sid, first, last = student
    url = "%s/students/%s" % (base, sid)

    data = do_get(url)
    if data:
        print "Update Student %s" % sid
    else:
        print "CREATE Student %s" % sid
        data = {}
        data["_id"] = sid
        data["UserID"] = sid
        data["TurnCount"] = 0
        data["AutoCreated"] = False

    data["FirstName"] = first
    data["LastName"] = last
    
    #Replace None's
    if not data.get("ReadingURLs", []):
        data["ReadingURLs"] = []

    do_post(url, data)

def main():
    args = sys.argv[1:]

    if len(args) != 1:
        return usage()

    base = args[0]
    print "Using API base: %s" % base

    global CLASSES, LESSONS, STUDENTS

    for s in STUDENTS:
        one_student(base, s)

    ss = [s[0] for s in STUDENTS]

    for l in LESSONS:
        one_lesson(base, l, ss)

    ll = [l[0] for l in LESSONS]

    for c in CLASSES:
        one_class(base, c, ll, ss)

if __name__ == "__main__":
    main()
